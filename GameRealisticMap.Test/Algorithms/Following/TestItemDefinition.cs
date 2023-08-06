using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Test.Algorithms.Following
{
    internal class TestItemDefinition : IItemDefinition<string>
    {
        public TestItemDefinition(double probability, float radius, string model, float? maxZ = null, float? minZ = null, float? maxScale = null, float? minScale = null)
        {
            Radius = radius;
            Model = model;
            MaxZ = maxZ;
            MinZ = minZ;
            MaxScale = maxScale;
            MinScale = minScale;
            Probability = probability;
        }

        public float Radius { get; }

        public string Model { get; }

        public float? MaxZ { get; }
        public float? MinZ { get; }

        public float? MaxScale { get; }

        public float? MinScale { get; }

        public double Probability { get; }
    }
}
