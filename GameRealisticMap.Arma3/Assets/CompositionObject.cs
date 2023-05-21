using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Assets
{
    [DebuggerDisplay("{Model} ({Transform.M41};{Transform.M43})")]
    public class CompositionObject
    {
        [JsonConstructor]
        public CompositionObject(ModelInfo model, Matrix4x4 transform)
        {
            Model = model;
            Transform = transform;
        }

        public ModelInfo Model { get; }

        [JsonConverter(typeof(Matrix4x4Converter))]
        public Matrix4x4 Transform { get; }

        public TerrainBuilderObject ToTerrainBuilderObject(Matrix4x4 matrix, ElevationMode mode)
        {
            return new TerrainBuilderObject(Model, Transform * matrix, mode);
        }
    }
}