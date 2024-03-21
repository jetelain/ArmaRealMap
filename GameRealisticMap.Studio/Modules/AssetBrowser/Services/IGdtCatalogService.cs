using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal interface IGdtCatalogStorage
    {
        Task<List<GdtCatalogItem>> GetOrLoad();

        Task SaveChanges(List<GdtCatalogItem> list);

        event EventHandler<List<GdtCatalogItem>> Updated;

        Task<GdtCatalogItem?> Resolve(string path);

        Task ImportMod(ModInfo installed);

        Task ImportVanilla();
    }
}
