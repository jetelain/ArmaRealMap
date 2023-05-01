using System.Numerics;
using GeoAPI.Geometries;

namespace GameRealisticMap.Geometries
{
    public class BoundingCircle : IBoundingShape
    {
        private readonly Lazy<TerrainPolygon> polygon;

        public BoundingCircle(TerrainPoint center, float radius, float angle)
        {
            Center = center;
            Radius = radius;
            Angle = angle;
            polygon = new Lazy<TerrainPolygon>(GeneratePolygon);
        }

        private TerrainPolygon GeneratePolygon()
        {
            return TerrainPolygon.FromCircle(Center, Radius);
        }

        public TerrainPoint Center { get; }

        public float Radius { get; }

        public float Angle { get; }

        public IPolygon Poly => polygon.Value.AsPolygon;

        public TerrainPoint MinPoint => Center - new Vector2(Radius, Radius);

        public TerrainPoint MaxPoint => Center + new Vector2(Radius, Radius);

        public TerrainPolygon Polygon => polygon.Value;
    }
}
