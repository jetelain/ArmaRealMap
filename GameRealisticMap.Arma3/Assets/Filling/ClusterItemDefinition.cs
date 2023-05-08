using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets.Filling
{
    public class ClusterItemDefinition : IClusterItemDefinition<Composition>
    {
        [JsonConstructor]
        public ClusterItemDefinition(float radius, float exclusiveRadius, Composition model, float? maxZ, float? minZ, double probability, float? maxScale, float? minScale)
        {
            Radius = radius;
            ExclusiveRadius = exclusiveRadius;
            Model = model;
            MaxZ = maxZ;
            MinZ = minZ;
            Probability = probability;
            MaxScale = maxScale;
            MinScale = minScale;
        }

        /// <summary>
        /// Radius between objects within area
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// Radius from the area limit (might be larger than Radius, but not smaller)
        /// </summary>
        public float ExclusiveRadius { get; }

        public Composition Model { get; }

        public float? MaxZ { get; }

        public float? MinZ { get; }

        public double Probability { get; }

        public float? MaxScale { get; }

        public float? MinScale { get; }

    }
}
