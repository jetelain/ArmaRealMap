using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    /// <summary>
    /// Downloads and installs Creator DLC substitute data packages so that the Studio
    /// can preview and generate maps that use Creator DLC assets without requiring
    /// the user to own those DLCs.
    /// </summary>
    internal interface ISubstituteDataService
    {
        Task EnsureDataInstalled(IEnumerable<ModInfo> mods);
    }
}
