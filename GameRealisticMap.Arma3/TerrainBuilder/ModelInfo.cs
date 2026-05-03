using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    /// <summary>
    /// Metadata for a single Arma 3 P3D model: its name, file path, and bounding box dimensions.
    /// Used by <see cref="IModelInfoLibrary"/> to resolve model references and by generators
    /// to match model sizes to building footprint dimensions.
    /// </summary>
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