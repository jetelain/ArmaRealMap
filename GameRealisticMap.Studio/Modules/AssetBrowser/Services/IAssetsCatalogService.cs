using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal interface IAssetsCatalogService
    {
        Task<List<AssetCatalogItem>> GetOrLoad();

        Task Save(List<AssetCatalogItem> items);

        Task<Dictionary<string, AssetCatalogItem>> GetItems(IEnumerable<string> paths);

        Task<List<AssetCatalogItem>> ImportItems(IEnumerable<string> paths, string modId = "");
    }
}
