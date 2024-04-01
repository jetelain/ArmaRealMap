using System.IO;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    public class GdtCatalogItem
    {
        public GdtCatalogItem(TerrainMaterial material, SurfaceConfig? config, GdtCatalogItemType itemType, string? title = null)
        {
            Material = material;
            Config = config;
            ItemType = itemType;
            Title = string.IsNullOrEmpty(title) ? Path.GetFileNameWithoutExtension(material.ColorTexture) : title;
        }

        public TerrainMaterial Material { get; }

        public SurfaceConfig? Config { get; }

        public GdtCatalogItemType ItemType { get; }

        public string Title { get; }
    }
}