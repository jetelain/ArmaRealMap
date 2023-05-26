using System.Text.Json.Serialization;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    public class AssetCatalogItem
    {
        [JsonConstructor]
        public AssetCatalogItem(string path, string modId, AssetCatalogCategory category)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            ModId = modId;
            Category = category;
        }

        public string Path { get; }

        [JsonIgnore]
        public string Name { get; }

        public string ModId { get; }

        public AssetCatalogCategory Category { get; set; }
    }
}