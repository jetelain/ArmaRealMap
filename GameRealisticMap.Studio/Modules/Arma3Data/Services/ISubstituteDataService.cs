using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface ISubstituteDataService
    {
        Task EnsureDataInstalled(IEnumerable<ModInfo> mods);
    }
}
