using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
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

        private const float SinCos45 = 0.70710678119f;

        private Polygon GeneratePolygon()
        {
            // Really basic circle approximation
            return new Polygon(new LinearRing(
                new[]
                {
                    new Coordinate(Center.X,                       Center.Y + Radius),
                    new Coordinate(Center.X + (Radius * SinCos45), Center.Y + (Radius * SinCos45)),
                    new Coordinate(Center.X + Radius,              Center.Y),
                    new Coordinate(Center.X + (Radius * SinCos45), Center.Y - (Radius * SinCos45)),
                    new Coordinate(Center.X,                       Center.Y - Radius),
                    new Coordinate(Center.X - (Radius * SinCos45), Center.Y - (Radius * SinCos45)),
                    new Coordinate(Center.X - Radius,              Center.Y),
                    new Coordinate(Center.X - (Radius * SinCos45), Center.Y + (Radius * SinCos45)),
                    new Coordinate(Center.X,                       Center.Y + Radius)
                }
                ));
        }

        public TerrainPoint Center { get; }

        public float Radius { get; }

        public float Angle { get; }

        public Polygon Poly => polygon.Value;

        public Vector2 StartPoint => Center.Vector - new Vector2(Radius, Radius);

        public Vector2 EndPoint => Center.Vector + new Vector2(Radius, Radius);
    }
}
