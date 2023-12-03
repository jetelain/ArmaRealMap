using System.Numerics;
using GameRealisticMap.Algorithms.RandomGenerators;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Filling
{
    public sealed class AreaDefinition : ITerrainEnvelope
    {
        internal AreaDefinition(TerrainPolygon polygon)
        {
            Polygon = polygon;
            MinPoint = polygon.MinPoint - new Vector2(2);
            MaxPoint = polygon.MaxPoint + new Vector2(2);
            Random = RandomHelper.CreateRandom(polygon.Centroid);
        }

        public TerrainPolygon Polygon { get; }

        internal Random Random { get; }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }
    }
}
