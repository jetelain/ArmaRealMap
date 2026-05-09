using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    /// <summary>
    /// Loads, saves, and queries the Studio asset catalog — a persisted list of
    /// Arma 3 P3D models enriched with category, bounding-box, and mod-ID metadata.
    /// The catalog is built by scanning installed mods and cached to disk.
    /// </summary>
    internal interface IAssetsCatalogService
    {
        Task<List<AssetCatalogItem>> GetOrLoad();

        Task Save(List<AssetCatalogItem> items);

        Task<Dictionary<string, AssetCatalogItem>> GetItems(IEnumerable<string> paths);

        Task<List<AssetCatalogItem>> ImportItems(IEnumerable<string> paths, string modId = "");
    }
}
