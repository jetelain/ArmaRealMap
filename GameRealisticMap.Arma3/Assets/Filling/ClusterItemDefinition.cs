using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class ClusterItemDefinition : IClusterItemDefinition<Composition>
    {
        public ClusterItemDefinition(float radius, float fitRadius, Composition model, float? maxZ, float? minZ, double probability, float? maxScale, float? minScale)
            : this(radius, fitRadius, null, model, maxZ, minZ, probability, maxScale, minScale)
        {

        }

        [JsonConstructor]
        public ClusterItemDefinition(float radius, float fitRadius, float? exclusiveRadius, Composition model, float? maxZ, float? minZ, double probability, float? maxScale, float? minScale)
        {
            Radius = radius;
            if (exclusiveRadius != null)
            {
                FitRadius = exclusiveRadius.Value;
            }
            else
            {
                FitRadius = fitRadius;
            }
            Model = model;
            Probability = probability;

            if (maxScale != null && minScale != null)
            {
                MaxScale = MathF.Max(maxScale.Value, minScale.Value);
                MinScale = MathF.Min(maxScale.Value, minScale.Value);
            }

            if (maxZ != null && minZ != null)
            {
                MaxZ = MathF.Max(maxZ.Value, minZ.Value);
                MinZ = MathF.Min(maxZ.Value, minZ.Value);
            }
        }

        /// <summary>
        /// Radius between objects within area
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// Radius from the area limit
        /// </summary>
        public float FitRadius { get; }


        /// <summary>
        /// Radius from the area limit
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [Obsolete]
        public float? ExclusiveRadius { get; }

        public Composition Model { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MaxZ { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MinZ { get; }

        public double Probability { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MaxScale { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public float? MinScale { get; }

    }
}
