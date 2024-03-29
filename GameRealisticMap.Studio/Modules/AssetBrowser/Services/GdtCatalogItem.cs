using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    public class GdtCatalogItem
    {
        public GdtCatalogItem(TerrainMaterial material, SurfaceConfig? config, GdtCatalogItemType itemType)
        {
            Material = material;
            Config = config;
            ItemType = itemType;
        }

        public TerrainMaterial Material { get; }

        public SurfaceConfig? Config { get; }

        public GdtCatalogItemType ItemType { get; }
    }
}