using System;
using System.Linq;
using System.Numerics;
using NetTopologySuite.Geometries;

namespace ArmaRealMap.Geometries
{
    internal class BoundingCircle : IBoundingShape
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

        public Vector2 StartPoint => Center.Vector - new Vector2(Radius, Radius);

        public Vector2 EndPoint => Center.Vector + new Vector2(Radius, Radius);
    }
}
