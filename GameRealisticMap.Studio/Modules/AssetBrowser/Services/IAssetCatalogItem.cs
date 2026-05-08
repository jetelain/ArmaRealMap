using System.Numerics;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    /// <summary>
    /// Exposes the category and 3D bounding-box dimensions of a catalogued Arma 3 asset.
    /// Used by the asset browser to filter models by category, and used by the map editor to determine the dimensions of the asset.
    /// </summary>
    public interface IAssetCatalogItem
    {
        AssetCatalogCategory Category { get; }

        Vector3 BboxMin { get; }

        Vector3 BboxMax { get; }
    }
}