using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ClipperLib;
using NetTopologySuite.Geometries;

namespace ArmaRealMap.Geometries
{
    public class TerrainPolygon : ITerrainGeometry
    {
        private static readonly List<List<TerrainPoint>> NoHoles = new List<List<TerrainPoint>>(0);

        private readonly Lazy<Polygon> asPolygon;

        public TerrainPolygon(List<TerrainPoint> shell, List<List<TerrainPoint>> holes)
        {
            this.Shell = shell;
            this.Holes = holes;
            MinPoint = new TerrainPoint(shell.Min(p => p.X), shell.Min(p => p.Y));
            MaxPoint = new TerrainPoint(shell.Max(p => p.X), shell.Max(p => p.Y));
            EnveloppeSize = MaxPoint.Vector - MinPoint.Vector;
            asPolygon = new Lazy<Polygon>(() => ToPolygon(c => new Coordinate(c.X, c.Y)));
        }

        public List<TerrainPoint> Shell { get; }

        public List<List<TerrainPoint>> Holes { get; }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        public Vector2 EnveloppeSize { get; }

        public Polygon ToPolygon(Func<TerrainPoint, Coordinate> project)
        {
            return new Polygon(
                new LinearRing(Shell.Select(project).ToArray()),
                Holes.Select(h => new LinearRing(h.Select(project).ToArray())).ToArray());
        }

        public static IEnumerable<TerrainPolygon> FromGeometry(Geometry geometry, Func<Coordinate, TerrainPoint> project)
        {
            switch(geometry.OgcGeometryType)
            {
                case OgcGeometryType.MultiPolygon:
                    return ((MultiPolygon)geometry).Geometries.SelectMany(p => FromGeometry(p, project));

                case OgcGeometryType.Polygon:
                    return FromPolygon((Polygon)geometry, project);

                case OgcGeometryType.LineString:
                    var line = (LineString)geometry;
                    if (line.IsClosed && line.Coordinates.Length > 4)
                    {
                        return new[] { new TerrainPolygon(line.Coordinates.Select(project).ToList(), NoHoles) };
                    }
                    break;
            }
            return new TerrainPolygon[0];
        }

        public static IEnumerable<TerrainPolygon> FromPolygon(Polygon polygon, Func<Coordinate,TerrainPoint> project)
        {
            if (!polygon.IsValid)
            {
                return MakePolygon(
                    polygon.ExteriorRing.Coordinates.Select(project).Select(p => p.ToIntPoint()).ToList(),
                    polygon.InteriorRings.Select(r => r.Coordinates.Select(project).Select(p => p.ToIntPoint()).ToList()).ToList());
            }
            return new[] {
                new TerrainPolygon(
                    polygon.ExteriorRing.Coordinates.Select(project).ToList(),
                    polygon.InteriorRings.Select(r => r.Coordinates.Select(project).ToList()).ToList())
            };
        }

        public static TerrainPolygon FromRectangleCentered(TerrainPoint center, Vector2 size, float degrees = 0.0f)
        {
            var points = GeometryHelper.RotatedRectangleDegrees(center.Vector, size, degrees).Select(v => new TerrainPoint(v));
            return new TerrainPolygon(points.Concat(points.Take(1)).ToList(), NoHoles);
        }

        public static TerrainPolygon FromRectangle(TerrainPoint start, TerrainPoint end)
        {
            return new TerrainPolygon(
                new List<TerrainPoint>()
                {
                    start,
                    new TerrainPoint(end.X, start.Y),
                    end,
                    new TerrainPoint(start.X, end.Y),
                    start
                },
                NoHoles);
        }

        public static TerrainPolygon FromCircle(TerrainPoint origin, float radius)
        {
            return new TerrainPolygon(
                GeometryHelper.SimpleCircle(origin.Vector, radius).Select(v => new TerrainPoint(v)).ToList(),
                NoHoles);
        }

        public static IEnumerable<TerrainPolygon> FromPath(IEnumerable<TerrainPoint> points, float width)
        {
            var clipper = new ClipperOffset();
            clipper.AddPath(points.Select(p => p.ToIntPoint()).ToList(), JoinType.jtSquare, EndType.etOpenSquare);
            var tree = new PolyTree();
            clipper.Execute(ref tree, width * GeometryHelper.ScaleForClipper / 2);
            return ToPolygons(tree);
        }

        public IEnumerable<TerrainPolygon> Offset(float offset)
        {
            var holes = Holes.SelectMany(r => OffsetInternal(r, -offset)).ToList();
            return OffsetInternal(Shell, offset).SelectMany(ext => MakePolygon(ext, holes)).ToList();
        }

        public IEnumerable<TerrainPolygon> Substract(List<TerrainPolygon> others)
        {
            if (others.Any(p => p.Holes.Count != 0))
            {
                throw new NotSupportedException();
            }

            var clipper = new Clipper();
            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            }
            foreach (var simple in others.Where(p => p.Holes.Count == 0))
            {
                if (GeometryHelper.EnveloppeIntersects(this, simple))
                {
                    clipper.AddPath(simple.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
                }
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            return ToPolygons(result);
        }
        public IEnumerable<TerrainPolygon> Merge(List<TerrainPolygon> others)
        {
            if (others.Any(p => p.Holes.Count != 0) || Holes.Count > 0)
            {
                throw new NotSupportedException();
            }

            var clipper = new Clipper();
            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            foreach (var simple in others.Where(p => p.Holes.Count == 0))
            {
                clipper.AddPath(simple.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctUnion, result, PolyFillType.pftPositive);
            return ToPolygons(result);
        }

        private static IEnumerable<TerrainPolygon> ToPolygons(PolyTree result)
        {
            return result.Childs.Select(c => new TerrainPolygon(ToRing(c.Contour), c.Childs.Select(h => ToRing(h.Contour)).ToList())).ToList();
        }

        public IEnumerable<TerrainPolygon> ClippedBy(TerrainPolygon other)
        {
            if (other.Holes.Count != 0)
            {
                throw new NotSupportedException();
            }
            var holes = Holes.SelectMany(r => ClipInternal(r, other.Shell)).ToList();
            return ClipInternal(Shell, other.Shell).SelectMany(ext => MakePolygon(ext, holes)).ToList();
        }

        private static List<List<IntPoint>> ClipInternal(List<TerrainPoint> shell, List<TerrainPoint> clip)
        {
            var clipper = new Clipper();
            clipper.AddPath(shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            clipper.AddPath(clip.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            if (result.Childs.Any(c => c.ChildCount != 0))
            {
                throw new NotSupportedException();
            }
            return result.Childs.Select(c => c.Contour).ToList();
        }

        public bool Contains(TerrainPoint point)
        {
            if (point.X <= MaxPoint.X && point.X >= MinPoint.X &&
                point.Y <= MaxPoint.Y && point.Y >= MinPoint.Y)
            {
                return ContainsRaw(point);
            }
            return false;
        }

        public bool ContainsRaw(TerrainPoint point)
        {
            if (PointInPolygon(Shell, point) == 1)
            {
                return Holes.All(hole => PointInPolygon(hole, point) == 0);
            }
            return false;
        }

        public TerrainPolygon ToShell()
        {
            return new TerrainPolygon(Shell, NoHoles);
        }

        private static IEnumerable<List<IntPoint>> OffsetInternal(List<TerrainPoint> p, double delta)
        {
            var clipper = new ClipperOffset();
            clipper.AddPath(p.Select(c => c.ToIntPoint()).ToList(), JoinType.jtSquare, EndType.etClosedPolygon);
            var tree = new List<List<IntPoint>>();
            clipper.Execute(ref tree, delta * GeometryHelper.ScaleForClipper);
            return tree;
        }

        public static int PointInPolygon(List<TerrainPoint> path, TerrainPoint pt)
        {
            //returns 0 if false, +1 if true, -1 if pt ON polygon boundary
            //See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
            //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
            int result = 0, cnt = path.Count;
            if (cnt < 3) return 0;
            TerrainPoint ip = path[0];
            for (int i = 1; i <= cnt; ++i)
            {
                TerrainPoint ipNext = (i == cnt ? path[0] : path[i]);
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

        private static IEnumerable<TerrainPolygon> MakePolygon(List<IntPoint> ext, List<List<IntPoint>> holes)
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
                return result.Childs.Select(c => new TerrainPolygon(ToRing(c.Contour), c.Childs.Select(h => ToRing(h.Contour)).ToList())).ToList();
            }
            return new[] { new TerrainPolygon(ToRing(ext), NoHoles) };
        }

        private static List<TerrainPoint> ToRing(List<IntPoint> points)
        {
            var transformedBack = points.Select(c => new TerrainPoint(c));
            return transformedBack.Concat(transformedBack.Take(1)).ToList();
        }
    }
}
