using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    [Export(typeof(IArma3Dependencies))]
    internal class Arma3Dependencies : IArma3Dependencies
    {
        private readonly IArma3DataModule _dataModule;
        private readonly IArma3ModsService _arma3ModsService;

        [ImportingConstructor]
        public Arma3Dependencies(IArma3DataModule dataModule, IArma3ModsService arma3ModsService)
        {
            _dataModule = dataModule;
            _arma3ModsService = arma3ModsService;
        }

        private IEnumerable<string> ComputeModDependenciesPath(IEnumerable<string> usedFiles)
        {
            return (_dataModule.ProjectDrive.SecondarySource as PboFileSystem)?.GetModPaths(usedFiles) ?? Enumerable.Empty<string>();
        }

        public IEnumerable<ModDependencyDefinition> ComputeModDependencies(IEnumerable<string> usedFiles)
        {
            var workshopPath = Arma3ToolsHelper.GetArma3WorkshopPath().TrimEnd('\\');

            // Detect creator DLC dependencies
            var cdlcList = new List<ModInfo>();
            foreach (var cdlc in _arma3ModsService.CreatorDlc)
            {
                var prefix = cdlc.Prefix + "\\";
                if (usedFiles.Any(f => f.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    cdlcList.Add(cdlc);
                }
            }

            return ComputeModDependenciesPath(usedFiles)
                .Select(path => GetSteamId(workshopPath, path))
                .Where(s => s != null)
                .Select(m => new ModDependencyDefinition(m!))
                .Concat(cdlcList.Select(c => new ModDependencyDefinition(c.SteamId!, Path.GetFileName(c.Path))))
                .ToList();
        }

        private string? GetSteamId(string workshopPath, string addonsPath)
        {
            var modPath = Path.GetDirectoryName(addonsPath);
            if (modPath != null)
            {
                var parent = Path.GetDirectoryName(modPath)?.TrimEnd('\\');
                if (string.Equals(parent, workshopPath, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFileName(modPath);
                }
            }
            return null;
        }
    }
}
