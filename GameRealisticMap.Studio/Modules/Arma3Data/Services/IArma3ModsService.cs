using System.Collections.Generic;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3ModsService
    {
        List<ModInfo> GetModsList();

        ModInfo? GetMod(string steamId);
    }
}
