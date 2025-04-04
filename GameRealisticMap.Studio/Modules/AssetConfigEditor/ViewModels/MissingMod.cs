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

        public string SteamUri =>  Arma3ModsService.CreatorDlcIds.Contains(SteamId) ? $"steam://purchase/{SteamId}" :  $"steam://url/CommunityFilePage/{SteamId}";

        public static async Task<List<MissingMod>> DetectMissingMods(IArma3DataModule arma3Data, IArma3ModsService mods, ISubstituteDataService substituteDataService, Arma3AssetsDependenciesOnly deps)
        {
            var activeMods = arma3Data.ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toEnable = new List<ModInfo>();
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
                    toEnable.Add(mod);
                }
            }
            if (toEnable.Count > 0)
            {
                await substituteDataService.EnsureDataInstalled(toEnable);
                await arma3Data.ChangeActiveMods(arma3Data.ActiveMods.Concat(toEnable.Select(m => m.Path)));
            }
            if (toInstallSteamId != null)
            {
                return toInstallSteamId.Select(m => new MissingMod(m)).ToList();
            }
            return new List<MissingMod>();
        }
    }
}