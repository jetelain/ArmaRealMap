using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public sealed class UniformRandomPointGenerator : IRandomPointGenerator
    {
        private readonly int rndX1;
        private readonly int rndX2;
        private readonly int rndY1;
        private readonly int rndY2;
        private readonly Random random;

        public UniformRandomPointGenerator(Random random, TerrainPoint min, TerrainPoint max)
        {
            this.random = random;
            rndX1 = (int)(min.X * 100);
            rndX2 = (int)(max.X * 100);
            rndY1 = (int)(min.Y * 100);
            rndY2 = (int)(max.Y * 100);
        }

        public Random Random => random;

        public TerrainPoint GetRandomPoint()
        {
            var x = random.Next(rndX1, rndX2) / 100f;
            var y = random.Next(rndY1, rndY2) / 100f;
            return new TerrainPoint(x, y);
        }
    }
}
