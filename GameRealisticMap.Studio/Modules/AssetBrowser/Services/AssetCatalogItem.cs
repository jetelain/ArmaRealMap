using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    public class AssetCatalogItem
    {
        [JsonConstructor]
        public AssetCatalogItem(string path, string modId, AssetCatalogCategory category, Vector3 size, float height)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            ModId = modId;
            Category = category;
            Size = size;
            Height = height;
        }

        public string Path { get; }

        [JsonIgnore]
        public string Name { get; }

        public string ModId { get; }

        public AssetCatalogCategory Category { get; set; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Size { get; }

        public float Height { get; }
    }
}