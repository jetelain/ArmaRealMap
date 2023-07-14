using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class MissingMod
    {
        public MissingMod(string steamId)
        {
            SteamId = steamId;
        }

        public string SteamId { get; }

        public string SteamUri => $"steam://url/CommunityFilePage/{SteamId}";

        public static async Task<List<MissingMod>> DetectMissingMods(IArma3DataModule arma3Data, IArma3ModsService mods, Arma3AssetsDependenciesOnly deps)
        {
            var activeMods = arma3Data.ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toEnablePaths = new List<string>();
            var toInstallSteamId = new List<string>();
            foreach (var dependency in deps.Dependencies)
            {
                var mod = mods.GetMod(dependency.SteamId);
                if (mod == null)
                {
                    // => NOT INSTALLED, must prompt user
                    toInstallSteamId.Add(dependency.SteamId);
                }
                else if (!activeMods.Contains(mod.Path))
                {
                    // => NOT ENABLED
                    toEnablePaths.Add(mod.Path);
                }
            }
            if (toEnablePaths.Count > 0)
            {
                await arma3Data.ChangeActiveMods(arma3Data.ActiveMods.Concat(toEnablePaths));
            }
            if (toInstallSteamId != null)
            {
                return toInstallSteamId.Select(m => new MissingMod(m)).ToList();
            }
            return new List<MissingMod>();
        }
    }
}