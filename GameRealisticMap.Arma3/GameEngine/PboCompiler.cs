using System.Runtime.Versioning;
using BIS.P3D.ODOL;
using BIS.PBO;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{

    [SupportedOSPlatform("windows")]
    public class PboCompiler : IPboCompiler
    {
        private readonly IProgressScope progress;
        private readonly ProjectDrive projectDrive;
        private readonly ModelInfoLibrary modelInfoLibrary;

        public PboCompiler(IProgressScope progress, ProjectDrive projectDrive, ModelInfoLibrary modelInfoLibrary)
        {
            this.progress = progress;
            this.projectDrive = projectDrive;
            this.modelInfoLibrary = modelInfoLibrary;
        }

        public async Task BinarizeAndCreatePbo(IPboConfig config, IReadOnlyCollection<string> usedModels, IReadOnlyCollection<string> usedRvmat)
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            var projectRoot = "P:";

            var tempRoot = Path.Combine(projectRoot, "grm-temp");
            var sourcePboPath = Path.Combine(projectRoot, config.PboPrefix);
            var tempPboPath = Path.Combine(tempRoot, config.PboPrefix);
            Directory.CreateDirectory(tempPboPath);

            var rvmat = Directory.GetFiles(Path.Combine(sourcePboPath, "data", "layers"), "*.rvmat");
            var targetRvmat = Path.Combine(tempPboPath, "data", "layers");
            await Arma3ToolsHelper.OptimizeRvmat(progress, rvmat, targetRvmat);

            var configCpp = Path.Combine(sourcePboPath, "config.cpp");
            var configSourceBin = Path.Combine(sourcePboPath, "config.bin");
            var configInitial = Path.Combine(sourcePboPath, "config-initial.hpp");
            var configSpecific = Path.Combine(sourcePboPath, "config-for-binarize.hpp");
            if (File.Exists(configCpp))
            {
                File.Copy(configCpp, configInitial, true);
                File.Delete(configCpp);
            }
            try
            {
                CreateFakeConfig(configSpecific, tempRoot, usedModels);

                await Arma3ToolsHelper.RunConfigConverter(progress, configSpecific, configSourceBin);

                using (var task = progress.CreateSingle("Binarize WRP"))
                {
                    await Arma3ToolsHelper.RunBinarize(task, $"-always -textures={tempRoot} -binPath={projectRoot} \"{sourcePboPath}\" \"{tempPboPath}\"");
                }
            }
            finally
            {
                File.Delete(configSourceBin);
                File.Copy(configInitial, configCpp, true);
            }

            var configTargetBin = Path.Combine(tempPboPath, "config.bin");
            await Arma3ToolsHelper.RunConfigConverter(progress, configInitial, configTargetBin);

            using (var task = progress.CreateSingle("Create PBO"))
            {
                CreatePbo(config, sourcePboPath, tempPboPath, configTargetBin);
            }

        }

        private void CreatePbo(IPboConfig config, string sourcePboPath, string targetPboPath, string configTargetBin)
        {
            var pbo = new PBO();
            pbo.PropertiesPairs.Add(new KeyValuePair<string, string>("prefix", config.PboPrefix));
            pbo.Files.Add(new PBOFileToAdd(new FileInfo(configTargetBin), "config.bin"));
            var sourceFiles = Directory.GetFiles(sourcePboPath, "*.*", SearchOption.AllDirectories);

            foreach (var sourceFile in sourceFiles)
            {
                var ext = Path.GetExtension(sourceFile);
                if (PackedFile.Contains(ext))
                {
                    var name = sourceFile.Substring(sourcePboPath.Length).TrimStart('\\');
                    var binarized = Path.Combine(targetPboPath, name);
                    if (File.Exists(binarized))
                    {
                        pbo.Files.Add(new PBOFileToAdd(new FileInfo(binarized), name));
                    }
                    else
                    {
                        pbo.Files.Add(new PBOFileToAdd(new FileInfo(sourceFile), name));
                    }
                }
            }

            var addonsPath = Path.Combine(config.TargetModDirectory, "addons");
            Directory.CreateDirectory(addonsPath);
            pbo.SaveTo(Path.Combine(addonsPath, Path.GetFileName(sourcePboPath) + ".pbo"));
        }

        private static readonly HashSet<string> PackedFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".paa",
            ".rvmat",
            ".wrp",
            ".dbf",
            ".shp",
            ".shx",
            ".cfg"
        };

        private void CreateFakeConfig(string targetFile, string tempRoot, IReadOnlyCollection<string> usedModels)
        {
            var sw = new StringWriter();
            sw.WriteLine("// GRM::ONLY FOR BINARIZE, you can safely ignore or delete this file");
            sw.WriteLine(@"#include ""config-initial.hpp""");
            sw.WriteLine("class cfgVehicles"); 
            sw.WriteLine("{");
            foreach (var model in usedModels.WithProgress(progress, "Prepare models"))
            {
                using (var sourceStream = projectDrive.OpenFileIfExists(model))
                {
                    if (sourceStream != null)
                    {
                        var modelTemp = Path.Combine(tempRoot, model);
                        Directory.CreateDirectory(Path.GetDirectoryName(modelTemp)!);
                        using (var targetStream = File.Create(modelTemp))
                        {
                            sourceStream.CopyTo(targetStream);
                        }
                    }
                }

                var odol = modelInfoLibrary.ReadODOL(model);
                if (odol != null)
                {
                    if (IsLandConfigRequired(odol))
                    {
                        sw.WriteLine($"class land_{Path.GetFileNameWithoutExtension(model)};");
                    }
                }
            }
            sw.WriteLine("};");
            File.WriteAllText(targetFile, sw.ToString());
        }

        private static readonly HashSet<string> ClassWithLandConfig = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "house",
            "church",
            "housesimulated",
            "tower"
        };


        private bool IsLandConfigRequired(ODOL odol)
        {
            if (ClassWithLandConfig.Contains(odol.ModelInfo.Class))
            {
                return true;
            }

            return false;
        }
    }
}
