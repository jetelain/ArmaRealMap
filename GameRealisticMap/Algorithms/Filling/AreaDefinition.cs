using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Filling
{
    public class AreaDefinition : ITerrainEnvelope
    {
        private readonly int rndX1;
        private readonly int rndX2;
        private readonly int rndY1;
        private readonly int rndY2;

        internal AreaDefinition(TerrainPolygon polygon)
        {
            Polygon = polygon;
            MinPoint = polygon.MinPoint - new Vector2(2);
            MaxPoint = polygon.MaxPoint + new Vector2(2);
            Random = RandomHelper.CreateRandom(polygon.Centroid);
            rndX1 = (int)(polygon.MinPoint.X * 100);
            rndX2 = (int)(polygon.MaxPoint.X * 100);
            rndY1 = (int)(polygon.MinPoint.Y * 100);
            rndY2 = (int)(polygon.MaxPoint.Y * 100);
        }

        public TerrainPolygon Polygon { get; }

        internal Random Random { get; }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        internal TerrainPoint GetRandomPoint()
        {
            var x = Random.Next(rndX1, rndX2) / 100f;
            var y = Random.Next(rndY1, rndY2) / 100f;
            return new TerrainPoint(x, y);
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
