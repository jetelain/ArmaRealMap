using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal interface IGdtCatalogService
    {
        Task<List<GdtCatalogItem>> GetOrLoad();

        Task SaveChanges(List<GdtCatalogItem> list);
    }
}
