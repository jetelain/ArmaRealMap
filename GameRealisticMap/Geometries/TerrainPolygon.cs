using System.Numerics;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Geometries
{
    public class TerrainPolygon : ITerrainEnvelope, IEquatable<TerrainPolygon>, ITerrainGeo
    {
        internal static readonly List<List<TerrainPoint>> NoHoles = new List<List<TerrainPoint>>(0);

        private readonly Lazy<Polygon> asPolygon;

        public TerrainPolygon(List<TerrainPoint> shell)
            : this(shell, NoHoles)
        {

        }

        [JsonConstructor]
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

        public IReadOnlyList<List<TerrainPoint>> Holes { get; }

        [JsonIgnore]
        public TerrainPoint MinPoint { get; }

        [JsonIgnore]
        public TerrainPoint MaxPoint { get; }

        [JsonIgnore]
        public Vector2 EnveloppeSize { get; }

        [JsonIgnore]
        public Polygon AsPolygon => asPolygon.Value;

        [JsonIgnore]
        public double Area => AsPolygon.Area;

        [JsonIgnore]
        public double EnveloppeArea => (MaxPoint.Vector - MinPoint.Vector).LengthSquared();

        [JsonIgnore]
        public TerrainPoint Centroid 
        { 
            get
            {
                var c = AsPolygon.Centroid;
                return new TerrainPoint((float)c.X, (float)c.Y);
            }
        }

        public Polygon ToPolygon(Func<TerrainPoint, Coordinate> project)
        {
            return new Polygon(
                new LinearRing(Shell.Select(project).ToArray()),
                Holes.Select(h => new LinearRing(h.Select(project).ToArray())).ToArray());
        }

        public static IEnumerable<TerrainPolygon> FromGeometry(IGeometry geometry, Func<Coordinate, TerrainPoint> project, float width = 6.0f)
        {
            switch(geometry.OgcGeometryType)
            {
                case OgcGeometryType.MultiPolygon:
                    return ((IMultiPolygon)geometry).Geometries.SelectMany(p => FromGeometry(p, project, width));

                case OgcGeometryType.Polygon:
                    return FromPolygon((IPolygon)geometry, project);

                case OgcGeometryType.LineString:
                    var line = (LineString)geometry;
                    if (line.IsClosed)
                    {
                        if (line.Coordinates.Length > 4)
                        {
                            return new[] { new TerrainPolygon(line.Coordinates.Select(project).ToList()) };
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

        public static IEnumerable<TerrainPolygon> FromPolygon(IPolygon polygon, Func<Coordinate,TerrainPoint> project)
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

        public static TerrainPolygon FromGeoJson(GeoJSON.Text.Geometry.Polygon polygon)
        {
            var shell = polygon.Coordinates.First();
            var holes = polygon.Coordinates.Skip(1);
            return new TerrainPolygon(
                    shell.Coordinates.Select(TerrainPoint.FromGeoJson).ToList(),
                    holes.Select(r => r.Coordinates.Select(TerrainPoint.FromGeoJson).ToList()).ToList());
        }

        public static TerrainPolygon FromRectangleCentered(TerrainPoint center, Vector2 size, float degrees = 0.0f)
        {
            var points = GeometryHelper.RotatedRectangleDegrees(center.Vector, size, degrees).Select(v => new TerrainPoint(v));
            return new TerrainPolygon(points.Concat(points.Take(1)).ToList());
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
                });
        }

        public static TerrainPolygon FromCircle(TerrainPoint origin, float radius)
        {
            return new TerrainPolygon(
                GeometryHelper.SimpleCircle(origin.Vector, radius).Select(v => new TerrainPoint(v)).ToList());
        }

        public static IEnumerable<TerrainPolygon> FromPath(IEnumerable<TerrainPoint> points, float width, EndType endType = EndType.etOpenRound)
        {
            var clipper = new ClipperOffset(2, 10);
            clipper.AddPath(points.Select(p => p.ToIntPoint()).ToList(), JoinType.jtSquare, endType);
            var tree = new PolyTree();
            clipper.Execute(ref tree, width * GeometryHelper.ScaleForClipper / 2);
            return ToPolygons(tree);
        }

        public IEnumerable<TerrainPolygon> Offset(float offset)
        {
            if (offset==0)
            {
                return new[] { this };
            }
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

        public IEnumerable<TerrainPolygon> OuterCrown(float outerOffset)
        {
            var clipper = new Clipper();

            foreach(var poly in Offset(outerOffset))
            {
                clipper.AddPath(poly.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
                foreach (var hole in poly.Holes)
                {
                    clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true); // EvenOdd will do the job
                }
            }

            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }

            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            return ToPolygons(result);
        }

        public IEnumerable<TerrainPolygon> InnerCrown(float innerOffset)
        {
            var clipper = new Clipper();

            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true); // EvenOdd will do the job
            }

            foreach (var poly in Offset(-innerOffset))
            {
                clipper.AddPath(poly.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
                foreach (var hole in poly.Holes)
                {
                    clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
                }
            }

            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            return ToPolygons(result);
        }

        public IEnumerable<TerrainPolygon> Crown(float outerOffset, float innerOffset)
        {
            if (outerOffset == 0)
            {
                return InnerCrown(innerOffset);
            }

            if (innerOffset == 0)
            {
                return OuterCrown(outerOffset);
            }

            var clipper = new Clipper();

            foreach (var poly in Offset(outerOffset))
            {
                clipper.AddPath(poly.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true);
                foreach (var hole in poly.Holes)
                {
                    clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, true); // EvenOdd will do the job
                }
            }

            foreach (var poly in Offset(-innerOffset))
            {
                clipper.AddPath(poly.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
                foreach (var hole in poly.Holes)
                {
                    clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
                }
            }

            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            return ToPolygons(result);
        }

        public IEnumerable<TerrainPolygon> SubstractAll(IEnumerable<TerrainPolygon> others)
        {
            var result = new List<TerrainPolygon>() { this };
            foreach(var other in others.Where(o => this.EnveloppeIntersects(o)))
            {
                var previousResult = result.ToList();
                result.Clear();
                foreach(var subjet in previousResult)
                {
                    result.AddRange(subjet.Substract(other));
                }
                if ( result.Count == 0)
                {
                    return result;
                }
            }
            return result;
        }

        public IEnumerable<TerrainPolygon> Substract(TerrainPolygon other)
        {
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

        private void ToClipper(Clipper clipper, PolyType type)
        {
            clipper.AddPath(Shell.Select(c => c.ToIntPoint()).ToList(), type, true);
            foreach (var hole in Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), type, true);
            }
        }

        public static List<TerrainPolygon> MergeAll(IReadOnlyCollection<TerrainPolygon> others, IProgressInteger? progress = null, double artefactFilter = 0.01f)
        {
            return MergeAllNew(others, progress, artefactFilter);
        }

        public static List<TerrainPolygon> MergeAllParallel(IReadOnlyCollection<TerrainPolygon> others, IProgressInteger? progress = null, double artefactFilter = 0.01f)
        {
            var scope = TerrainPolygonsHelper.GetEnvelope(others);
            return GeometryHelper.ParallelMerge(scope, others, 100, l => MergeAllNew(l, null, artefactFilter), progress).GetAwaiter().GetResult();
        }

        public static List<TerrainPolygon> MergeAllNew(IReadOnlyCollection<TerrainPolygon> others, IProgressInteger? progress = null, double artefactFilter = 0.01f)
        {
            if (others.Count == 0)
            {
                return new List<TerrainPolygon>(0);
            }
            var source = others.Where(p => p.Area > artefactFilter).ToList();
            var merged = new List<TerrainPolygon>(source.Take(1));
            var box = merged.GetEnvelope();
            var position = 1;
            foreach (var polygon in source.Skip(1))
            {
                if (polygon.EnveloppeIntersects(box) && box != Envelope.None)
                {
                    var clipper = new Clipper();
                    foreach (var other in merged)
                    {
                        other.ToClipper(clipper, PolyType.ptSubject);
                    }
                    polygon.ToClipper(clipper, PolyType.ptClip);

                    var result = new PolyTree();
                    clipper.Execute(ClipType.ctUnion, result);
                    merged = ToPolygons(result).Where(p => p.Area > artefactFilter).ToList();
                }
                else
                {
                    merged.Add(polygon);
                }
                box = merged.GetEnvelope();
                progress?.ReportOneDone();
                position++;
            }
            return merged;
        }

        public static List<TerrainPolygon> MergeAllOld(List<TerrainPolygon> others, IProgressPercent? progress = null, double artefactFilter = 0.01f)
        {
            var tomerge = others.ToList();

            /*var noholes = others.Where(p => p.Holes.Count == 0).ToList();
            var tomerge = others.Where(p => p.Holes.Count != 0).ToList();
            */
            /*if (noholes.Count > 0)
            {
                // Polygons without holes can easily merged
                if (tomerge.Count == 0 )
                {
                    return QuickMergeAllWithNoHoles(noholes);
                }
                tomerge.AddRange(QuickMergeAllWithNoHoles(noholes));
            }*/

            if (tomerge.Count > 1)
            {
                var changed = false;
                do {
                    changed = false;
                    foreach(var poly in tomerge)
                    {
                        foreach(var other in tomerge.Where(other => other != poly && GeometryHelper.EnveloppeIntersects(poly, other)))
                        {
                            var merged = poly.Merge(other, artefactFilter).ToList();
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
            progress?.Report(100);
            return tomerge;
        }

        public IEnumerable<TerrainPolygon> Merge(TerrainPolygon other, double artefactFilter = 0.01f)
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
            return ToPolygons(result).Where(p => p.Area > artefactFilter).ToList();
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
            return FromPolyTreeNode(result).ToList();
        }

        private static IEnumerable<TerrainPolygon> FromPolyTreeNode(PolyNode result)
        {
            return result.Childs.Select(c => new TerrainPolygon(ToRing(c.Contour), c.Childs.Select(h => ToRing(h.Contour)).ToList()))
                .Concat(result.Childs.SelectMany(c => c.Childs).SelectMany(FromPolyTreeNode));
        }

        public IEnumerable<TerrainPolygon> ClippedByEnveloppe(ITerrainEnvelope other)
        {
            return ClippedBy(FromRectangle(other.MinPoint, other.MaxPoint));
        }

        public IEnumerable<TerrainPolygon> ClippedBy(TerrainPolygon other)
        {
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

        public bool ContainsIncludingBorder(TerrainPoint point)
        {
            if (point.X <= MaxPoint.X && point.X >= MinPoint.X &&
                point.Y <= MaxPoint.Y && point.Y >= MinPoint.Y)
            {
                if (PointInPolygon(Shell, point) != 0)
                {
                    return Holes.All(hole => PointInPolygon(hole, point) == 0);
                }
            }
            return false;
        }

        public bool ContainsOrSimilar(TerrainPolygon other)
        {
            if (GeometryHelper.EnveloppeIntersects(this, other))
            {
                return AsPolygon.Contains(other.AsPolygon) || Shell.SequenceEqual(other.Shell);
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
            return new TerrainPolygon(Shell);
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
                return ToPolygons(result);
            }
            return new[] { new TerrainPolygon(ToRing(ext)) };
        }

        private static List<TerrainPoint> ToRing(List<IntPoint> points)
        {
            var transformedBack = points.Select(c => new TerrainPoint(c));
            return transformedBack.Concat(transformedBack.Take(1)).ToList();
        }

        public bool Equals(TerrainPolygon? other)
        {
            if (other == null)
            {
                return false;
            }
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as TerrainPolygon);
        }

        public override int GetHashCode()
        {
            return (Holes.Count, Shell.Count, Shell.First()).GetHashCode();
        }

        public override string ToString()
        {
            return AsPolygon.ToString();
        }

        public GeoJSON.Text.Geometry.Polygon ToGeoJson(Func<TerrainPoint, GeoJSON.Text.Geometry.IPosition> project)
        {
            return new GeoJSON.Text.Geometry.Polygon(
                new[] { new GeoJSON.Text.Geometry.LineString(Shell.Select(project).ToList()) }.Concat(Holes.Select(h => new GeoJSON.Text.Geometry.LineString(h.Select(project).ToList()))));
        }

        public float Distance(TerrainPoint point)
        {
            return (float)AsPolygon.Distance(new Point(point.X, point.Y));
        }

        public float DistanceFromBoundary(TerrainPoint p)
        {
            var px = new Point(p.X, p.Y);
            var shells = new[] { new DistanceOp(AsPolygon.Shell, px) }.Concat(AsPolygon.Holes.Select(h => new DistanceOp(h, px)));
            return (float)shells.Min(s => s.Distance());
        }

        public TerrainPoint NearestPointBoundary(TerrainPoint p)
        {
            var distance = new DistanceOp(AsPolygon, new Point(p.X, p.Y));
            var segment = distance.NearestPoints();
            if (distance.Distance() == 0f)
            {
                var shells = new[] { new DistanceOp(AsPolygon.Shell, new Point(p.X, p.Y)) }.Concat(AsPolygon.Holes.Select(h => new DistanceOp(h, new Point(p.X, p.Y))));
                segment = shells.OrderBy(s => s.Distance()).First().NearestPoints();
            }
            return new TerrainPoint((float)segment[0].X, (float)segment[0].Y);
        }


        public bool Intersects(TerrainPolygon other)
        {
            if (GeometryHelper.EnveloppeIntersects(this, other))
            {
                return AsPolygon.Intersects(other.AsPolygon);
            }
            return false;
        }

        public double IntersectionArea(TerrainPolygon other)
        {
            return Intersection(other).Sum(o => o.Area);
        }

        internal bool Contains(TerrainPath path)
        {
            if (GeometryHelper.EnveloppeIntersects(this, path))
            {
                return AsPolygon.Contains(path.AsLineString);
            }
            return false;
        }
    }
}
