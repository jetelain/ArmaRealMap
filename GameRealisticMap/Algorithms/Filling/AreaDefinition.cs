using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Filling
{
    internal class AreaDefinition : ITerrainEnvelope
    {
        private readonly int rndX1;
        private readonly int rndX2;
        private readonly int rndY1;
        private readonly int rndY2;

        public AreaDefinition(TerrainPolygon polygon)
        {
            Polygon = polygon;
            MinPoint = polygon.MinPoint - new Vector2(2);
            MaxPoint = polygon.MaxPoint + new Vector2(2);
            Random = new Random((int)Math.Truncate(polygon.Centroid.X + polygon.Centroid.Y));
            rndX1 = (int)(polygon.MinPoint.X * 100);
            rndX2 = (int)(polygon.MaxPoint.X * 100);
            rndY1 = (int)(polygon.MinPoint.Y * 100);
            rndY2 = (int)(polygon.MaxPoint.Y * 100);
        }

        public TerrainPolygon Polygon { get; }

        public Random Random { get; }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        public TerrainPoint GetRandomPoint()
        {
            var x = Random.Next(rndX1, rndX2) / 100f;
            var y = Random.Next(rndY1, rndY2) / 100f;
            return new TerrainPoint(x, y);
        }

        public TerrainPoint GetRandomPointInside()
        {
            var point = GetRandomPoint();
            while (!Polygon.Contains(point))
            {
                point = GetRandomPoint();
            }
            return point;
        }
    }
}
