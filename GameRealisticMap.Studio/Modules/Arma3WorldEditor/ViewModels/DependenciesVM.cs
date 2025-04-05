using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using NLog;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal sealed class DependenciesVM : PropertyChangedBase
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("Arma3WorldEditor");

        private readonly Arma3WorldEditorViewModel parent;
        private readonly IArma3ModsService arma3ModsService;
        private List<ModInfo> _mods = new List<ModInfo>();
        private List<ModDependencyDefinition> _items = new List<ModDependencyDefinition>();

        public DependenciesVM(Arma3WorldEditorViewModel parent, IArma3ModsService arma3ModsService)
        {
            this.parent = parent;
            this.arma3ModsService = arma3ModsService;
        }

        public List<ModDependencyDefinition> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    _items = value;
                    NotifyOfPropertyChange();
                    if (value != null)
                    {
                        _mods = value.Select(d => arma3ModsService.GetMod(d.SteamId) ?? new ModInfo(d.SteamId, "", d.SteamId)).ToList();
                        NotifyOfPropertyChange(nameof(Mods));
                    }
                    else
                    {
                        _mods = new List<ModInfo>();
                        NotifyOfPropertyChange(nameof(Mods));
                    }
                }
            }
        }

        public List<ModInfo> Mods => _mods;

        internal static string DependenciesFilePath(string filePath)
        {
            return Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, "grma3-dependencies.json");
        }

        internal async Task Save(string wrpFilePath)
        {
            using var stream = File.Create(DependenciesFilePath(wrpFilePath));
            await JsonSerializer.SerializeAsync(stream, Items);
        }

        internal async Task Load(string wrpFilePath)
        {
            Items = await ReadFromFile(DependenciesFilePath(wrpFilePath));
        }

        internal async Task<List<ModDependencyDefinition>> ReadFromFile(string dependenciesFile)
        {
            if (File.Exists(dependenciesFile))
            {
                try
                {
                    using var stream = File.OpenRead(dependenciesFile);
                    return await JsonSerializer.DeserializeAsync<List<ModDependencyDefinition>>(stream) ?? new List<ModDependencyDefinition>();
                }
                catch(Exception e)
                {
                    logger.Error(e, "Error reading dependencies file {0}", dependenciesFile);
                }
            }
            return Detect();
        }

        private List<ModDependencyDefinition> Detect()
        {
            var world = parent.World;
            var materials = parent.Materials;

            if (world != null)
            {
                var usedFiles = world.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct().ToList();

                if (materials != null)
                {
                    var libraryItems = materials
                        .Select(m => m.LibraryItem)
                        .Where(m => m != null && m.IsGameData)
                        .Distinct()
                        .ToList();

                    usedFiles.AddRange(libraryItems.Select(m => m!.NormalTexture));
                    usedFiles.AddRange(libraryItems.Select(m => m!.ColorTexture));

                    usedFiles.AddRange(materials.Where(m => m.LibraryItem == null).Select(m => m.ColorTexture));
                    usedFiles.AddRange(materials.Where(m => m.LibraryItem == null).Select(m => m.NormalTexture));
                }

                // XXX: This is incomplete, as it does not include config files dependencies

                return IoC.Get<IArma3Dependencies>()
                    .ComputeModDependencies(usedFiles)
                    .ToList();
            }
            return new List<ModDependencyDefinition>();
        }

        /// <summary>
        /// Recompute the dependencies, As config depdendencies are not detected, this might remove a required dependency.
        /// </summary>
        public void Recompute()
        {
            Items = Detect();
        }

        /// <summary>
        /// Recompute the dependencies, but only add new ones.
        /// </summary>
        public void RecomputeIncremental()
        {
            var detected = Detect();
            Items = detected.Concat(Items.Where(i => !detected.Any(d => d.SteamId == i.SteamId))).ToList();
        }
    }
}
