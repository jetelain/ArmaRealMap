using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.IO.Converters;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public class ModelInfo
    {
        [JsonConstructor]
        public ModelInfo(string name, string path, string bundle, Vector3 boundingCenter)
        {
            Name = name;
            Path = path;
            Bundle = bundle;
            BoundingCenter = boundingCenter;
        }

        public string Name { get; }

        public string Path { get; }

        public string Bundle { get; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 BoundingCenter { get; }  
    }
}