using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Randomizations;

namespace GameRealisticMap.Arma3.Assets
{
    [DebuggerDisplay("{Model} ({Transform.M41};{Transform.M43})")]
    public class CompositionObject
    {
        [JsonConstructor]
        public CompositionObject(ModelInfo model, Matrix4x4 transform, List<IRandomizationOperation>? randomizations = null)
        {
            Model = model;
            Transform = transform;
            Randomizations = randomizations;
        }

        public ModelInfo Model { get; }

        [JsonConverter(typeof(Matrix4x4Converter))]
        public Matrix4x4 Transform { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<IRandomizationOperation>? Randomizations { get; }

        public TerrainBuilderObject ToTerrainBuilderObject(Matrix4x4 matrix, ElevationMode mode, Random? parentRandom = null)
        {
            if (Randomizations != null && Randomizations.Count > 0)
            {
                var modelCenter = new Vector3(Transform.M41, Transform.M42, Transform.M43);
                var random = parentRandom ?? RandomHelper.CreateRandom(new Geometries.TerrainPoint(matrix.M41, matrix.M43));
                var randomization = Randomizations[0].GetMatrix(random, modelCenter);
                foreach (var operation in Randomizations.Skip(1))
                {
                    randomization = randomization * operation.GetMatrix(random, modelCenter);
                }
                return new TerrainBuilderObject(Model, Transform * randomization * matrix, mode);
            }
            return new TerrainBuilderObject(Model, Transform * matrix, mode);
        }

        public TerrainBuilderObject ToTerrainBuilderObjectVerbatim()
        {
            return new TerrainBuilderObject(Model, Transform, ElevationMode.Absolute);
        }
    }
}