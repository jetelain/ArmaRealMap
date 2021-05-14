using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Union;

namespace ArmaRealMap.Geometries
{
    public static class GeometryHelper
    {
        internal static IEnumerable<Polygon> LatLngToTerrainPolygon(MapInfos map, Geometry geometry)
        {
            return ToPolygon(geometry, list => map.LatLngToTerrainPoints(list).Select(p => new Coordinate(p.X, p.Y)));
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
    }
}
