using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.IO.Converters;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    [DebuggerDisplay("{Name}")]
    public class ModelInfo
    {
        [JsonConstructor]
        public ModelInfo(string name, string path, Vector3 boundingCenter)
        {
            Name = name;
            Path = path;
            BoundingCenter = boundingCenter;
        }

        public string Name { get; }

        public string Path { get; }

        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 BoundingCenter { get; }  
    }
}