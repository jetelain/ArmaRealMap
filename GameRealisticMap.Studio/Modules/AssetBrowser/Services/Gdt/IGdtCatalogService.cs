using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal interface IGdtCatalogStorage
    {
        Task<List<GdtCatalogItem>> GetOrLoad();

        Task SaveChanges(List<GdtCatalogItem> list);

        event EventHandler<List<GdtCatalogItem>> Updated;

        Task ImportMod(ModInfo installed);

        Task ImportVanilla();
    }
}
