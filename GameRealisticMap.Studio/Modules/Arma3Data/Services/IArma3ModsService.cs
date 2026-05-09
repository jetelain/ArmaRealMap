using System.Collections.Generic;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    /// <summary>
    /// Provides access to Arma 3 mods installed in the user's Steam library.
    /// Scans the Arma 3 launcher config to build the list of active mods and Creator DLC.
    /// </summary>
    internal interface IArma3ModsService
    {
        List<ModInfo> GetModsList();

        ModInfo? GetMod(string steamId); 

        IReadOnlyList<ModInfo> CreatorDlc { get; }
    }
}
