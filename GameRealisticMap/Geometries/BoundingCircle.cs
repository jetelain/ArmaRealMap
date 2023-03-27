using System.Numerics;
using NetTopologySuite.Geometries;

namespace GameRealisticMap.Geometries
{
    public class BoundingCircle : IBoundingShape
    {
        private readonly Lazy<Polygon> polygon;

        public BoundingCircle(TerrainPoint center, float radius, float angle)
        {
            Center = center;
            Radius = radius;
            Angle = angle;
            polygon = new Lazy<Polygon>(GeneratePolygon);
        }

        private Polygon GeneratePolygon()
        {
            return new Polygon(new LinearRing(GeometryHelper.SimpleCircle(Center.Vector, Radius).Select(p => new Coordinate(p.X, p.Y)).ToArray()));
        }

        public TerrainPoint Center { get; }

        public float Radius { get; }

        public float Angle { get; }

        public Polygon Poly => polygon.Value;

        public TerrainPoint MinPoint => Center - new Vector2(Radius, Radius);

        public TerrainPoint MaxPoint => Center + new Vector2(Radius, Radius);
    }
}
