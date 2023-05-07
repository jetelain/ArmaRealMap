using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Arma3.Assets
{
    internal class ClusterItemDefinition : IClusterItemDefinition<Composition>
    {
        public ClusterItemDefinition(float radius, Composition model, float? maxZ, float? minZ, double probability, float? maxScale, float? minScale)
        {
            Radius = radius;
            Model = model;
            MaxZ = maxZ;
            MinZ = minZ;
            Probability = probability;
            MaxScale = maxScale;
            MinScale = minScale;
        }

        public float Radius { get; }

        public Composition Model { get; }

        public float? MaxZ { get; }

        public float? MinZ { get; }

        public double Probability { get; }

        public float? MaxScale { get; }

        public float? MinScale { get; }
    }
}
