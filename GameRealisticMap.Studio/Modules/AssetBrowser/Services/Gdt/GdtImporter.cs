using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BIS.Core.Config;
using BIS.PBO;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using NLog;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal class GdtImporter
    {
        private static readonly Logger logger = LogManager.GetLogger("GdtImporter");

        private static readonly string[] WellKnownVanillaConfig = new[]
        {
            @"Argo\Addons\map_malden.pbo",
            @"Enoch\Addons\map_enoch.pbo",
            @"Expansion\Addons\map_tanoabuka.pbo",
            @"Addons\map_altis.pbo",
            @"Addons\map_stratis.pbo",
            @"Addons\map_data.pbo"
        };
        private readonly IModelInfoLibrary modelInfoLibrary;

        internal const string DefaultPattern = "gdt_*_co.paa";

        internal GdtImporter(IModelInfoLibrary modelInfoLibrary)
        {
            this.modelInfoLibrary = modelInfoLibrary;
        }

        internal List<GdtImporterItem> ImportVanilla(PboFileSystem pboFS)
        {
            var basePath = Arma3ToolsHelper.GetArma3Path();

            var pboToScanForConfig = WellKnownVanillaConfig.Select(p => Path.Combine(basePath, p));

            return Import(pboFS, pboToScanForConfig, DefaultPattern);
        }

        internal List<GdtImporterItem> ImportMod(ModInfo mod)
        {
            var pattern = DefaultPattern; 
            if (mod.SteamId == "2982306133")
            {
                pattern = "arm_*_co.paa";
            }
            return ImportMod(mod.Path, pattern);
        }

        internal List<GdtImporterItem> ImportMod(string modPath, string pattern = DefaultPattern)
        {
            var pboFS = new PboFileSystem(Enumerable.Empty<string>(), new[] { modPath });

            return Import(pboFS, Directory.GetFiles(modPath, "*.pbo", SearchOption.AllDirectories), pattern);
        }

        private List<GdtImporterItem> Import(PboFileSystem pboFS, IEnumerable<string> pboToScanForConfig, string pattern)
        {
            var parsedSurfaces = GetSurfacesFromConfig(pboToScanForConfig);

            var colorTextureFiles = pboFS.FindAll(pattern).ToList();

            var result = new List<GdtImporterItem>();

            foreach (var colorTexture in colorTextureFiles)
            {
                var normalTexture = colorTexture.Replace("_co.paa", "_nopx.paa");
                if (pboFS.Exists(normalTexture))
                {
                    var name = Path.GetFileNameWithoutExtension(colorTexture);

                    var config = parsedSurfaces
                        .Where(s => s.Match(name))
                        .FirstOrDefault();

                    if (config != null)
                    {
                        result.Add(new GdtImporterItem(colorTexture, normalTexture, config));
                    }
                    else
                    {
                        logger.Warn($"No config found for '{colorTexture}'");
                    }
                }
                else
                {
                    logger.Warn($"Normal texture not found for '{colorTexture}'");
                }
            }

            return result;
        }

        private List<SurfaceConfig> GetSurfacesFromConfig(IEnumerable<string> pboToScanForConfig)
        {
            var surfaces = new List<ParamClass>();
            var characters = new List<ParamClass>();
            var clutters = new List<ParamClass>();

            IndexConfig(pboToScanForConfig, surfaces, characters, clutters);

            var parsedSurfaces = new List<SurfaceConfig>();
            foreach (var surface in surfaces)
            {
                var files = surface.GetValue<string>("files");
                if (!string.IsNullOrEmpty(files))
                {
                    parsedSurfaces.Add(new SurfaceConfig(surface.Name,
                        aceCanDig: surface.GetValue<double>("ACE_canDig") == 1,
                        files,
                        soundEnviron: surface.GetValue<string>("soundEnviron"),
                        soundHit: surface.GetValue<string>("soundHit"),
                        rough: surface.GetValue<double>("rough"),
                        maxSpeedCoef: surface.GetValue<double>("maxSpeedCoef", 1),
                        dust: surface.GetValue<double>("dust"),
                        lucidity: surface.GetValue<double>("lucidity"),
                        grassCover: surface.GetValue<double>("grassCover"),
                        impact: surface.GetValue<string>("impact"),
                        surfaceFriction: surface.GetValue<double>("surfaceFriction"),
                        maxClutterColoringCoef: surface.GetValue<double>("maxClutterColoringCoef", 1),
                        GetClutters(characters, clutters, surface.GetValue<string>("character"))));
                }
            }
            return parsedSurfaces;
        }

        private void IndexConfig(IEnumerable<string> pboToScanForConfig, List<ParamClass> surfaces, List<ParamClass> characters, List<ParamClass> clutters)
        {
            foreach (var pboPath in pboToScanForConfig)
            {
                using var pbo = new PBO(pboPath);

                var config = pbo.GetRootConfig();

                if (config != null)
                {
                    FillIndex(surfaces, config.Root.GetClass("CfgSurfaces"));

                    FillIndex(characters, config.Root.GetClass("CfgSurfaceCharacters"));

                    var worlds = config.Root.GetClass("CfgWorlds");
                    if (worlds != null)
                    {
                        foreach (var world in worlds.Entries.OfType<ParamClass>())
                        {
                            FillIndex(clutters, world.GetClass("clutter"));
                        }
                    }
                }
            }
        }

        private List<ClutterConfig> GetClutters(List<ParamClass> characters, List<ParamClass> clutters, string character)
        {
            var cluttersForSurface = new List<ClutterConfig>();
            var match = characters
                .Where(c => string.Equals(c.Name, character, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(c => c.Entries.Count)
                .FirstOrDefault();
            if (match != null)
            {
                var probabilities = match.GetArray<double>("probability");
                var names = match.GetArray<string>("names");

                for (int i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    var probability = i < probabilities.Length ? probabilities[i] : 0;
                    var matchingClutter = clutters
                        .Where(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(c => c.Entries.Count)
                        .FirstOrDefault();
                    if (matchingClutter != null)
                    {
                        var model = matchingClutter.GetValue<string>("model");
                        if (!string.IsNullOrEmpty(model) && modelInfoLibrary.TryResolveByPath(model, out var modelInfo))
                        {
                            cluttersForSurface.Add(new ClutterConfig(name, probability, modelInfo,
                                affectedByWind: matchingClutter.GetValue<double>("affectedByWind", 1),
                                isSwLighting: matchingClutter.GetValue<double>("swLighting", 1) == 1,
                                scaleMin: matchingClutter.GetValue<double>("scaleMin", 1),
                                scaleMax: matchingClutter.GetValue<double>("scaleMax", 1))); ;
                        }
                    }
                    else
                    {
                        logger.Warn($"No clutter was found for name '{name}'.");
                    }
                }
            }
            return cluttersForSurface;
        }

        private void FillIndex(List<ParamClass> index, ParamClass? paramClass)
        {
            if (paramClass != null)
            {
                index.AddRange(paramClass.Entries.OfType<ParamClass>());
            }
        }
    }
}
