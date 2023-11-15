using System;
using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    public sealed class AssetCatalogItem : IAssetCatalogItem
    {
        [JsonConstructor]
        public AssetCatalogItem(string path, string modId, AssetCatalogCategory category, Vector3 size, float height, Vector3? bboxMin, Vector3? bboxMax, Vector3? boundingCenter, DateTime? timestamp)
        {
            Path = path;
            Name = System.IO.Path.GetFileName(path);
            ModId = modId;
            Category = category;
            Size = size;
            Height = height;
            BboxMax = bboxMax;
            BboxMin = bboxMin;
            BoundingCenter = boundingCenter;
            Timestamp = timestamp;
        }

        public string Path { get; }

        [JsonIgnore]
        public string Name { get; }

        public string ModId { get; }

        public AssetCatalogCategory Category { get; set; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Size { get; }

        public float Height { get; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3? BboxMin { get; set; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3? BoundingCenter { get; set; }

        public DateTime? Timestamp { get; set; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3? BboxMax { get; set; }

        [JsonIgnore]
        Vector3 IAssetCatalogItem.BboxMin => BboxMin ?? (Size / -2);

        [JsonIgnore]
        Vector3 IAssetCatalogItem.BboxMax => BboxMax ?? (Size / 2);

        [JsonIgnore]
        public bool HasBounding => BboxMax != null && BboxMin != null && BoundingCenter != null;
    }
}