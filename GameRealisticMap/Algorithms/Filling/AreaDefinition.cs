using System.Numerics;
using GameRealisticMap.Algorithms.RandomGenerators;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Filling
{
    public sealed class AreaDefinition : ITerrainEnvelope
    {
        private readonly IRandomPointGenerator randomPointGenerator;

        internal AreaDefinition(TerrainPolygon polygon)
        {
            Polygon = polygon;
            MinPoint = polygon.MinPoint - new Vector2(2);
            MaxPoint = polygon.MaxPoint + new Vector2(2);
            Random = RandomHelper.CreateRandom(polygon.Centroid);
            randomPointGenerator = new UniformRandomPointGenerator(Random, polygon.MinPoint, polygon.MaxPoint);
        }

        public TerrainPolygon Polygon { get; }

        internal Random Random { get; }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        internal TerrainPoint GetRandomPoint()
        {
            return randomPointGenerator.GetRandomPoint();
        }

        internal TerrainPoint? GetRandomPointInside()
        {
            var point = GetRandomPoint();
            int attempts = 0;
            while (!Polygon.Contains(point))
            {
                point = GetRandomPoint();
                attempts++;
                if (attempts > 200_000) // Safeguard
                {
                    return null;
                }
            }
            return point;
        }
    }
}
