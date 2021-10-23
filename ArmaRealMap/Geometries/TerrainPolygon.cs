using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ClipperLib;
using NetTopologySuite.Geometries;

namespace ArmaRealMap.Geometries
{
    public class TerrainPolygon : ITerrainGeometry, IEquatable<TerrainPolygon>
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

        public Polygon AsPolygon => asPolygon.Value;

        public Polygon ToPolygon(Func<TerrainPoint, Coordinate> project)
        {
            return new Polygon(
                new LinearRing(Shell.Select(project).ToArray()),
                Holes.Select(h => new LinearRing(h.Select(project).ToArray())).ToArray());
        }

        public static IEnumerable<TerrainPolygon> FromGeometry(Geometry geometry, Func<Coordinate, TerrainPoint> project, float width = 6.0f)
        {
            switch(geometry.OgcGeometryType)
            {
                case OgcGeometryType.MultiPolygon:
                    return ((MultiPolygon)geometry).Geometries.SelectMany(p => FromGeometry(p, project, width));

                case OgcGeometryType.Polygon:
                    return FromPolygon((Polygon)geometry, project);

                case OgcGeometryType.LineString:
                    var line = (LineString)geometry;
                    if (line.IsClosed)
                    {
                        if (line.Coordinates.Length > 4)
                        {
                            return new[] { new TerrainPolygon(line.Coordinates.Select(project).ToList(), NoHoles) };
                        }
                    }
                    else
                    {
                        return FromPath(line.Coordinates.Select(project).ToList(), width);
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
        public IEnumerable<TerrainPolygon> Crown(float width)
        {
            return Crown(width/2, Shell).Concat(Holes.SelectMany(h => Crown(width/2f, h))).ToList();
        }

        private static IEnumerable<TerrainPolygon> Crown(float offset, List<TerrainPoint> points)
        {
            var exterior = FromPath(points, offset);
            var interior = FromPath(points, -offset);
            return exterior.SelectMany(e => e.SubstractAll(interior));
        }

        public IEnumerable<TerrainPolygon> SubstractAll(IEnumerable<TerrainPolygon> others)
        {
            var result = new List<TerrainPolygon>() { this };
            foreach(var other in others.Where(o => GeometryHelper.EnveloppeIntersects(this, o)))
            {
                var previousResult = result.ToList();
                result.Clear();
                foreach(var subjet in previousResult)
                {
                    result.AddRange(subjet.Substract(other));
                }
            }
            return result;
        }

        public IEnumerable<TerrainPolygon> Substract(TerrainPolygon other)
        {
            if (!GeometryHelper.EnveloppeIntersects(this, other))
            {
                return new[] { this };
            }

            var clipper = new Clipper();
            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true); // EvenOdd will do the job
            }
            clipper.AddPath(other.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in other.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            return ToPolygons(result);
        }

        public static List<TerrainPolygon> MergeAll(List<TerrainPolygon> others)
        {
            var noholes = others.Where(p => p.Holes.Count == 0).ToList();
            var tomerge = others.Where(p => p.Holes.Count != 0).ToList();

            if (noholes.Count > 0)
            {
                // Polygons without holes can easily merged
                if (tomerge.Count == 0 )
                {
                    return QuickMergeAllWithNoHoles(noholes);
                }
                tomerge.AddRange(QuickMergeAllWithNoHoles(noholes));
            }

            if (tomerge.Count > 1)
            {
                var changed = false;
                do {
                    changed = false;
                    foreach(var poly in tomerge)
                    {
                        foreach(var other in tomerge.Where(other => other != poly && GeometryHelper.EnveloppeIntersects(poly, other)))
                        {
                            var merged = poly.Merge(other).ToList();
                            if (merged.Count == 1) // successfully merged
                            {
                                tomerge.Remove(poly);
                                tomerge.Remove(other);
                                tomerge.AddRange(merged);
                                changed = true;
                                break;
                            }
                        }
                        if (changed)
                        {
                            break;
                        }
                    }
                }
                while (changed);
            }
            return tomerge;
        }

        private static List<TerrainPolygon> QuickMergeAllWithNoHoles(List<TerrainPolygon> noholes)
        {
            var clipper = new Clipper();
            foreach (var simple in noholes)
            {
                clipper.AddPath(simple.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctUnion, result, PolyFillType.pftPositive);
            return ToPolygons(result);
        }

        public IEnumerable<TerrainPolygon> Merge(TerrainPolygon other)
        {
            if (!GeometryHelper.EnveloppeIntersects(this, other))
            {
                return new[] { this, other };
            }

            var clipper = new Clipper();
            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true); // EvenOdd will do the job
            }
            clipper.AddPath(other.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in other.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctUnion, result);
            return ToPolygons(result);
        }

        public IEnumerable<TerrainPolygon> Intersection(TerrainPolygon other)
        {
            if (!GeometryHelper.EnveloppeIntersects(this, other))
            {
                return new TerrainPolygon[0];
            }

            var clipper = new Clipper();
            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true); // EvenOdd will do the job
            }
            clipper.AddPath(other.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in other.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctIntersection, result);
            return ToPolygons(result);
        }

        private static List<TerrainPolygon> ToPolygons(PolyTree result)
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
            clipper.Execute(ClipType.ctIntersection, result);
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

        public bool Contains(TerrainPolygon other)
        {
            if (GeometryHelper.EnveloppeIntersects(this, other))
            {
                return AsPolygon.Contains(other.AsPolygon);
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

        public bool Equals(TerrainPolygon other)
        {
            if (Holes.Count != other.Holes.Count)
            {
                return false;
            }
            if (!Shell.SequenceEqual(other.Shell))
            {
                return false;
            }
            for(int i = 0; i < Holes.Count; ++i)
            {
                if (!Holes[i].SequenceEqual(other.Holes[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TerrainPolygon);
        }
    }
}
