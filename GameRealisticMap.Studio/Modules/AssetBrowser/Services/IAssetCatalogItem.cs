using System.Numerics;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    public interface IAssetCatalogItem
    {
        AssetCatalogCategory Category { get; }

        Vector3 BboxMin { get; }

        Vector3 BboxMax { get; }
    }
}