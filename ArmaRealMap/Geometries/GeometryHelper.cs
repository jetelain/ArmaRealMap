using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ClipperLib;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Union;

namespace ArmaRealMap.Geometries
{
    public static class GeometryHelper
    {
        internal const double ScaleForClipper = 1000d;

        internal static IEnumerable<Polygon> LatLngToTerrainPolygon(MapInfos map, Geometry geometry)
        {
            return ToPolygon(geometry, list => map.LatLngToTerrainPoints(list).Select(p => new Coordinate(p.X, p.Y)));
        }

        internal static IEnumerable<Polygon> Offset(Polygon p, double delta)
        {
            var interiors = p.InteriorRings.SelectMany(r => OffsetInternal(r, -delta)).ToList();
            return OffsetInternal(p.ExteriorRing, delta).SelectMany(ext => MakePolygon(ext, interiors)).ToList();
        }

        private static IEnumerable<List<IntPoint>> OffsetInternal(LineString p, double delta)
        {
            var clipper = new ClipperOffset();
            clipper.AddPath(p.Coordinates.Select(c => new IntPoint(c.X * ScaleForClipper, c.Y * ScaleForClipper)).ToList(), JoinType.jtSquare, EndType.etClosedPolygon);
            var tree = new List<List<IntPoint>>();
            clipper.Execute(ref tree, delta * ScaleForClipper);
            return tree;
        }

        private static LinearRing ToRing(List<IntPoint> points)
        {
            var transformedBack = points.Select(c => new Coordinate(c.X / ScaleForClipper, c.Y / ScaleForClipper));
            return new LinearRing(transformedBack.Concat(transformedBack.Take(1)).ToArray());
        }

        private static IEnumerable<Polygon> MakePolygon(List<IntPoint> ext, List<List<IntPoint>> holes)
        {
            if (holes.Any())
            {
                var clipper = new Clipper();
                clipper.AddPath(ext, PolyType.ptSubject, true);
                foreach (var hole in holes)
                {
                    clipper.AddPath(hole, PolyType.ptClip, true);
                }
                var result = new PolyTree();
                clipper.Execute(ClipType.ctDifference, result);
                return result.Childs.Select(c => new Polygon(ToRing(c.Contour), c.Childs.Select(h => ToRing(h.Contour)).ToArray())).ToList();
            }
            return new[] { new Polygon(ToRing(ext), holes.Select(ToRing).ToArray()) };
        }

        internal static IEnumerable<Polygon> ToPolygon(Geometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<Coordinate>> transform)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                return ((MultiPolygon)geometry).Geometries.SelectMany(p => ToPolygon(p, transform));
            }
            if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (Polygon)geometry;
                return new[]
                {
                    new Polygon(
                        ToLinearRing(poly.ExteriorRing, transform),
                        poly.InteriorRings.Select(h => ToLinearRing(h, transform)).ToArray())
                };
            }
            if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (LineString)geometry;
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

        private static LinearRing ToLinearRing(LineString line, Func<IEnumerable<Coordinate>, IEnumerable<Coordinate>> transform)
        {
            return new LinearRing(transform(line.Coordinates).ToArray());
        }

        internal static IEnumerable<Polygon> CropPolygonsToMap(MapInfos map, IEnumerable<Polygon> polygons)
        {
            return polygons.SelectMany(p => CropPolygonToMap(map, p));
        }

        internal static IEnumerable<Polygon> CropPolygonToMap(MapInfos map, Polygon polygon)
        {
            var outer = CropRingToMap(map, polygon.ExteriorRing.Coordinates);
            if (outer != null)
            {
                var newPolygon =
                    new Polygon(
                        new LinearRing(outer.ToArray()),
                        polygon.InteriorRings.Select(h => CropRingToMap(map, h.Coordinates)).Where(h => h != null).Select(h => new LinearRing(h.ToArray())).ToArray());

                return new[] { newPolygon };
            }
            return new Polygon[0];
        }

        public static List<Coordinate> CropRingToMap(MapInfos map, IList<Coordinate> ring)
        {
            var firstPoint = ring.FirstOrDefault(a => map.IsInside(a));
            if (firstPoint == null)
            {
                return null;
            }
            var list = new List<Coordinate>() { firstPoint };
            var index = ring.IndexOf(firstPoint);
            var otherPoints = ring.Skip(index + 1).Concat(ring.Take(index+1));
            var previousWasInside = true;
            var previous = firstPoint;
            foreach(var point in otherPoints)
            {
                if (map.IsInside(point))
                {
                    if (!previousWasInside)
                    {
                        var previousAtBoundary = list[list.Count - 1];
                        var atBoundary = PointAtBoundary(map, point, previous);
                        if (!(previousAtBoundary.X == atBoundary.X || previousAtBoundary.Y == atBoundary.Y))
                        {
                            AddIfNotSameAsLast(list, new Coordinate(
                                (previousAtBoundary.X == map.P1.X || atBoundary.X == map.P1.X ? map.P1.X : map.P2.X),
                                (previousAtBoundary.Y == map.P1.Y || atBoundary.Y == map.P1.Y ? map.P1.Y : map.P2.Y)));
                        }
                        AddIfNotSameAsLast(list, atBoundary);
                    }
                    AddIfNotSameAsLast(list, point);
                    previousWasInside = true;
                }
                else
                {
                    if (previousWasInside)
                    {
                        var atBoundary = PointAtBoundary(map, previous, point);
                        AddIfNotSameAsLast(list, atBoundary);
                    }
                    previousWasInside = false;
                }
                previous = point;
            }
            return list;
        }

        internal static bool EnveloppeIntersects(ITerrainGeometry a, ITerrainGeometry b)
        {
            return b.MinPoint.X <= a.MaxPoint.X &&
                b.MinPoint.Y <= a.MaxPoint.Y &&
                b.MaxPoint.X >= a.MinPoint.X &&
                b.MaxPoint.Y >= a.MinPoint.Y;
        }

        private static void AddIfNotSameAsLast(List<Coordinate> list, Coordinate coordinate)
        {
            if (!list[list.Count-1].Equals(coordinate))
            {
                list.Add(coordinate);
            }
        }

        public static Coordinate PointAtBoundary(MapInfos map, Coordinate inside, Coordinate outside)
        {
            var dx = (outside.X - inside.X);
            var dy = (outside.Y - inside.Y);
            var candidates = new List<Coordinate>();
            if (outside.Y <= map.P1.Y) 
            {
                candidates.Add(new Coordinate(inside.X + (dx * (map.P1.Y - inside.Y) / dy), map.P1.Y));
            }
            if (outside.Y >= map.P2.Y) 
            {
                candidates.Add(new Coordinate(inside.X + (dx * (map.P2.Y - inside.Y) / dy), map.P2.Y));
            }
            if (outside.X <= map.P1.X)
            {
                candidates.Add(new Coordinate(map.P1.X, inside.Y + (dy * (map.P1.X - inside.X) / dx)));
            }
            if (outside.X >= map.P2.X)
            {
                candidates.Add(new Coordinate(map.P2.X, inside.Y + (dy * (map.P2.X - inside.X) / dx)));
            }
            return candidates.First(c => c.X >= map.P1.X && c.X <= map.P2.X && c.Y >= map.P1.Y && c.Y <= map.P2.Y);
        }

        public static IEnumerable<Polygon> ToValidPolygon(Polygon item)
        {
            var failed = false;
            try
            {
                item.Contains(item.Centroid);
            }
            catch
            {
                failed = true;
            }
            if (failed)
            {
                var exteriorRing = new LinearRing(item.ExteriorRing.Coordinates);
                var exteriorPolygon = new Polygon(exteriorRing);
                if (exteriorPolygon.IsValid)
                {
                    return new[] { new Polygon(exteriorRing, MergeHoles(item).ToArray()) };
                }
                else
                {
                    // Bad news
                    return new Polygon[0];
                }
            }
            else
            {
                // Ok for Contains
                return new[] { item };
            }
        }

        private static List<LinearRing> MergeHoles(Polygon item)
        {
            var holes = item.InteriorRings.Cast<LinearRing>().ToList();
            foreach (var hole in holes.ToList())
            {
                if (holes.Contains(hole))
                {
                    var touches = holes.Where(h => h != hole && h.Intersects(hole)).ToList();
                    if (touches.Count > 0)
                    {
                        holes.Remove(hole);
                        foreach (var touch in touches)
                        {
                            holes.Remove(touch);
                        }
                        touches.Add(hole);
                        var merged = UnaryUnionOp.Union(touches.Select(r => new Polygon(r))) as Polygon;
                        if (merged != null)
                        {
                            holes.Add((LinearRing)merged.ExteriorRing);
                        }
                    }
                }
            }
            return holes;
        }

        public static IEnumerable<Polygon> EnsureValidPolygons(IEnumerable<Polygon> list)
        {
            return list.Where(p => p.IsValid).Concat(list.Where(p => !p.IsValid).SelectMany(ToValidPolygon));
        }

        internal static IEnumerable<LineString> LatLngToLineString(MapInfos map, Geometry geometry)
        {
            return ToLineString(geometry, list => map.LatLngToTerrainPoints(list).Select(p => new Coordinate(p.X, p.Y)));
        }

        public static IEnumerable<LineString> ToLineString(Geometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<Coordinate>> transform)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var points = transform(((LineString)geometry).Coordinates).ToArray();
                if (points.Length > 1)
                {
                    return new[] { new LineString(points) };
                }
                return new LineString[0];
            }
            throw new ArgumentException(geometry.OgcGeometryType.ToString());
        }


        public static int PointInPolygon(Coordinate[] path, Coordinate pt)
        {
            //returns 0 if false, +1 if true, -1 if pt ON polygon boundary
            //See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
            //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
            int result = 0, cnt = path.Length;
            if (cnt < 3) return 0;
            Coordinate ip = path[0];
            for (int i = 1; i <= cnt; ++i)
            {
                Coordinate ipNext = (i == cnt ? path[0] : path[i]);
                if (ipNext.Y == pt.Y)
                {
                    if ((ipNext.X == pt.X) || (ip.Y == pt.Y && ((ipNext.X > pt.X) == (ip.X < pt.X))))
                        return -1;
                }
                if ((ip.Y < pt.Y) != (ipNext.Y < pt.Y))
                {
                    if (ip.X >= pt.X)
                    {
                        if (ipNext.X > pt.X) result = 1 - result;
                        else
                        {
                            double d = (double)(ip.X - pt.X) * (ipNext.Y - pt.Y) - (double)(ipNext.X - pt.X) * (ip.Y - pt.Y);
                            if (d == 0) return -1;
                            else if ((d > 0) == (ipNext.Y > ip.Y)) result = 1 - result;
                        }
                    }
                    else
                    {
                        if (ipNext.X > pt.X)
                        {
                            double d = (double)(ip.X - pt.X) * (ipNext.Y - pt.Y) - (double)(ipNext.X - pt.X) * (ip.Y - pt.Y);
                            if (d == 0) return -1;
                            else if ((d > 0) == (ipNext.Y > ip.Y)) result = 1 - result;
                        }
                    }
                }
                ip = ipNext;
            }
            return result;
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

        private static IEnumerable<TerrainPoint> PointsOnPath(List<TerrainPoint>  points, TerrainPoint a, TerrainPoint b, float step = 1f)
        {
            var delta = b.Vector - a.Vector;
            var length = delta.Length();
            for (float i = step; i < length; i += step)
            {
                var point = new TerrainPoint(Vector2.Lerp(a.Vector, b.Vector, i / length));
                if (points.Last() != point)
                {
                    points.Add(point);
                }
            }
            if (points.Last() != b)
            {
                points.Add(b);
            }
            return points;
        }
    }
}
