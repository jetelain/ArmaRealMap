using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public sealed class MixedRandomPointGenerator : IRandomPointGenerator
    {
        private int diffuse;
        private readonly IRandomPointGenerator[] generators;

        public MixedRandomPointGenerator(params IRandomPointGenerator[] generators)
        {
            this.generators = generators;
        }

        public TerrainPoint GetRandomPoint()
        {
            diffuse = (diffuse + 1) % generators.Length;
            return generators[diffuse].GetRandomPoint();
        }
    }
}
