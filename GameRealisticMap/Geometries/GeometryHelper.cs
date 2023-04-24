using System.Numerics;
using ClipperLib;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace GameRealisticMap.Geometries
{
    public static class GeometryHelper
    {
        internal const double ScaleForClipper = 1000d;

        public static readonly Matrix3x2 Rotate90 = Matrix3x2.CreateRotation(1.57079637f);
        public static readonly Matrix3x2 RotateM90 = Matrix3x2.CreateRotation(-1.57079637f);

        public static IEnumerable<Polygon> ToPolygon(IGeometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<Coordinate>> transform)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                return ((IMultiPolygon)geometry).Geometries.SelectMany(p => ToPolygon(p, transform));
            }
            if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (IPolygon)geometry;
                return new[]
                {
                    new Polygon(
                        ToLinearRing(poly.ExteriorRing, transform),
                        poly.InteriorRings.Select(h => ToLinearRing(h, transform)).ToArray())
                };
            }
            if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (ILineString)geometry;
                if (line.IsClosed && line.Coordinates.Length > 4)
                {
                    return new[]
                    {
                        new Polygon(ToLinearRing(line, transform))
                    };
                }
            }
            return new Polygon[0];
        }

        private static LinearRing ToLinearRing(ILineString line, Func<IEnumerable<Coordinate>, IEnumerable<Coordinate>> transform)
        {
            return new LinearRing(transform(line.Coordinates).ToArray());
        }

        public static bool EnveloppeIntersects(this ITerrainEnvelope a, ITerrainEnvelope b)
        {
            return b.MinPoint.X <= a.MaxPoint.X &&
                b.MinPoint.Y <= a.MaxPoint.Y &&
                b.MaxPoint.X >= a.MinPoint.X &&
                b.MaxPoint.Y >= a.MinPoint.Y;
        }

        public static Vector2[] RotatedRectangleDegrees(Vector2 center, Vector2 size, float degrees)
        {
            return RotatedRectangleRadians(center, size, (float)(Math.PI * degrees / 180));
        }

        public static Vector2[] RotatedRectangleRadians(Vector2 center, Vector2 size, float radians)
        {
            var matrix = Matrix3x2.CreateRotation(radians, center);
            var halfSize = size / 2;
            var p1 = center - halfSize;
            var p3 = center + halfSize;
            return new[]
            {
                Vector2.Transform(p1,matrix),
                Vector2.Transform(new Vector2(p3.X, p1.Y),matrix),
                Vector2.Transform(p3,matrix),
                Vector2.Transform(new Vector2(p1.X, p3.Y),matrix)
            };
        }

        private static readonly float Cos60Sin30 = 0.5f;
        private static readonly float Sin60Cos30 = 0.8660254f;

        public static Vector2[] SimpleCircle(Vector2 center, float radius)
        {
            return new[] {
                new Vector2(center.X, center.Y + radius),
                new Vector2(center.X + (radius * Cos60Sin30), center.Y + (radius * Sin60Cos30)),
                new Vector2(center.X + (radius * Sin60Cos30), center.Y + (radius * Cos60Sin30)),
                new Vector2(center.X + radius, center.Y),
                new Vector2(center.X + (radius * Sin60Cos30), center.Y - (radius * Cos60Sin30)),
                new Vector2(center.X + (radius * Cos60Sin30), center.Y - (radius * Sin60Cos30)),
                new Vector2(center.X, center.Y - radius),
                new Vector2(center.X - (radius * Cos60Sin30), center.Y - (radius * Sin60Cos30)),
                new Vector2(center.X - (radius * Sin60Cos30), center.Y - (radius * Cos60Sin30)),
                new Vector2(center.X - radius, center.Y),
                new Vector2(center.X - (radius * Sin60Cos30), center.Y + (radius * Cos60Sin30)),
                new Vector2(center.X - (radius * Cos60Sin30), center.Y + (radius * Sin60Cos30)),
                new Vector2(center.X, center.Y + radius)
            };
        }


        public static IEnumerable<TerrainPoint> PointsOnPath(IEnumerable<TerrainPoint> points, float step = 1f)
        {
            var prev = points.First();
            var result = new List<TerrainPoint>() { prev };
            foreach (var point in points.Skip(1))
            {
                PointsOnPath(result, prev, point, step);
                prev = point;
            }
            return result;
        }

        private static IEnumerable<TerrainPoint> PointsOnPath(List<TerrainPoint> points, TerrainPoint a, TerrainPoint b, float step = 1f)
        {
            var delta = b.Vector - a.Vector;
            var length = delta.Length();
            for (float i = step; i < length; i += step)
            {
                var point = new TerrainPoint(Vector2.Lerp(a.Vector, b.Vector, i / length));
                if (!TerrainPoint.Equals(points.Last(),point))
                {
                    points.Add(point);
                }
            }
            if (!TerrainPoint.Equals(points.Last(),b))
            {
                points.Add(b);
            }
            return points;
        }


        public static List<TerrainPoint> PointsOnPathRegular(IEnumerable<TerrainPoint> points, float step)
        {
            /*var previous = points.First();
            var result = new List<TerrainPoint>() { previous };
            var remainLength = step;
            foreach (var point in points.Skip(1))
            {
                var delta = point.Vector - previous.Vector;
                var length = delta.Length();
                float positionOnSegment = remainLength;
                while (positionOnSegment <= length)
                {
                    result.Add(new TerrainPoint(Vector2.Lerp(previous.Vector, point.Vector, positionOnSegment / length)));
                    positionOnSegment += step;
                }
                remainLength = positionOnSegment - length;
                previous = point;
            }
            if (result.Last() != previous)
            {
                result.Add(previous);
            }
            return result;*/
            var follow = new FollowPath(points);
            var result = new List<TerrainPoint>() { follow.Current };
            while (follow.Move(step))
            {
                result.Add(follow.Current);
            }
            return result;
        }

        public static ITerrainEnvelope WithMargin(this ITerrainEnvelope geometry, float margin)
        {
            return WithMargin(geometry, new Vector2(margin));
        }

        public static ITerrainEnvelope WithMargin(this ITerrainEnvelope geometry, Vector2 margin)
        {
            return new GameRealisticMap.Geometries.Envelope(geometry.MinPoint - margin, geometry.MaxPoint + margin);
        }

        public static TerrainPoint KeepAway(TerrainPoint point, IEnumerable<ITerrainGeo> from, float minDistance = 2f)
        {
            var envelope = point.WithMargin(minDistance);
            var near = from.Where(p => p.EnveloppeIntersects(envelope)).ToList();
            foreach (var polygon in near)
            {
                var distance = polygon.Distance(point);
                if (distance == 0) // Inside
                {
                    var nearest = polygon.NearestPointBoundary(point);
                    var delta = nearest.Vector - point.Vector;
                    if (delta.X != 0 || delta.Y != 0)
                    {
                        var vector = Vector2.Normalize(delta) * minDistance;
                        point = point + vector;
                    }
                }
                else if (distance < minDistance) // Outside
                {
                    var nearest = polygon.NearestPointBoundary(point);
                    var vector = Vector2.Normalize(nearest.Vector - point.Vector) * (minDistance - distance);
                    point = point - vector;
                }
            }
            return point;
        }

        public static float? GetFacing(TerrainPoint point, IEnumerable<ITerrainGeo> facing, float maxDistance = 50f)
        {
            var envelope = point.WithMargin(maxDistance);
            var closest = facing.Where(p => p.EnveloppeIntersects(envelope)).MinBy(g => g.Distance(point));
            if (closest == null)
            {
                return null;
            }
            var facingPoint = closest.NearestPointBoundary(point);
            return VectorHelper.GetAngleFromYAxisInDegrees(facingPoint.Vector - point.Vector);
        }

    }
}
