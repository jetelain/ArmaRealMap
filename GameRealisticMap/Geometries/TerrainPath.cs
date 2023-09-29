using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoAPI.Geometries;
using MapToolkit;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace GameRealisticMap.Geometries
{
    [DebuggerDisplay("{Length}m {FirstPoint} to {LastPoint}")]
    public class TerrainPath : ITerrainEnvelope, ITerrainGeo
    {
        private readonly Lazy<LineString> asLineString;

        public TerrainPath(params TerrainPoint[] points)
            : this(points.ToList())
        {
        }

        [JsonConstructor]
        public TerrainPath(List<TerrainPoint> points)
        {
            this.Points = points;
            MinPoint = new TerrainPoint(points.Min(p => p.X), points.Min(p => p.Y));
            MaxPoint = new TerrainPoint(points.Max(p => p.X), points.Max(p => p.Y));
            EnveloppeSize = MaxPoint.Vector - MinPoint.Vector;
            asLineString = new Lazy<LineString>(() => ToLineString(c => new Coordinate(c.X, c.Y)));
        }

        public List<TerrainPoint> Points { get; } // TODO: Change to IReadOnlyList<TerrainPoint>

        [JsonIgnore]
        public TerrainPoint FirstPoint => Points[0];

        [JsonIgnore]
        public TerrainPoint LastPoint => Points[Points.Count - 1];

        [JsonIgnore]
        public TerrainPoint MinPoint { get; }

        [JsonIgnore]
        public TerrainPoint MaxPoint { get; }

        [JsonIgnore]
        public Vector2 EnveloppeSize { get; }

        [JsonIgnore]
        public LineString AsLineString => asLineString.Value;

        [JsonIgnore]
        public float Length => GetLength(Points);

        internal static float GetLength(IReadOnlyList<TerrainPoint> points)
        {
            var length = 0f;
            var prev = points[0];
            TerrainPoint point;
            for (int i = 1; i < points.Count; ++i)
            {
                point = points[i];
                length += (point.Vector - prev.Vector).Length();
                prev = point;
            }
            return length;
        }

        public LineString ToLineString(Func<TerrainPoint, Coordinate> project)
        {
            return new LineString(Points.Select(project).ToArray());
        }

        public IEnumerable<TerrainPolygon> ToTerrainPolygon(float width)
        {
            return TerrainPolygon.FromPath(Points, width);
        }
        public IEnumerable<TerrainPolygon> ToTerrainPolygonButt(float width)
        {
            return TerrainPolygon.FromPath(Points, width, EndType.etOpenButt);
        }

        public static IEnumerable<TerrainPath> FromGeometry(IGeometry geometry, Func<Coordinate, TerrainPoint> project)
        {
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.LineString:
                    return new[] { new TerrainPath(((ILineString)geometry).Coordinates.Select(project).ToList()) };
            }
            return new TerrainPath[0];
        }

        private List<TerrainPath> Intersection(TerrainPolygon polygon)
        {
            var clipper = new Clipper();
            clipper.AddPath(Points.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, false);
            clipper.AddPath(polygon.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in polygon.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctIntersection, result);
            if (result.Childs.Any(c => c.ChildCount != 0))
            {
                throw new NotSupportedException();
            }
            return result.Childs.Select(c => new TerrainPath(c.Contour.Select(p => new TerrainPoint(p)).ToList())).ToList();
        }

        public List<TerrainPath> Substract(TerrainPolygon polygon)
        {
            if (!EnveloppeIntersects(polygon))
            {
                return new List<TerrainPath>() { this };
            }
            var clipper = new Clipper();
            clipper.AddPath(Points.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, false);
            clipper.AddPath(polygon.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in polygon.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            if (result.Childs.Any(c => c.ChildCount != 0))
            {
                throw new NotSupportedException();
            }
            return result.Childs.Select(c => new TerrainPath(c.Contour.Select(p => new TerrainPoint(p)).ToList())).ToList();
        }


        public IEnumerable<TerrainPath> SubstractAll(IEnumerable<TerrainPolygon> others)
        {
            var result = new List<TerrainPath>() { this };
            foreach (var other in others.Where(o => GeometryHelper.EnveloppeIntersects(this, o)))
            {
                var previousResult = result.ToList();
                result.Clear();
                foreach (var subjet in previousResult)
                {
                    result.AddRange(subjet.Substract(other));
                }
            }
            return result;
        }

        public List<TerrainPath> SubstractKeepOrientation(TerrainPolygon polygon)
        {
            return KeepOrientation(Substract(polygon));
        }

        public IEnumerable<TerrainPath> SubstractAllKeepOrientation(IEnumerable<TerrainPolygon> others)
        {
            var result = new List<TerrainPath>() { this };
            foreach (var other in others.Where(o => GeometryHelper.EnveloppeIntersects(this, o)))
            {
                var previousResult = result.ToList();
                result.Clear();
                foreach (var subjet in previousResult)
                {
                    result.AddRange(subjet.SubstractKeepOrientation(other));
                }
            }
            return result;
        }

        public IEnumerable<TerrainPath> ClippedByEnveloppe(ITerrainEnvelope other)
        {
            if (!EnveloppeIntersects(other))
            {
                return Enumerable.Empty<TerrainPath>();
            }
            return Intersection(TerrainPolygon.FromRectangle(other.MinPoint, other.MaxPoint));
        }

        public IEnumerable<TerrainPath> ClippedBy(TerrainPolygon polygon)
        {
            if (!EnveloppeIntersects(polygon))
            {
                return Enumerable.Empty<TerrainPath>();
            }
            return Intersection(polygon);
        }

        public bool EnveloppeIntersects(ITerrainEnvelope other)
        {
            return other.MinPoint.X <= MaxPoint.X &&
                other.MinPoint.Y <= MaxPoint.Y &&
                other.MaxPoint.X >= MinPoint.X &&
                other.MaxPoint.Y >= MinPoint.Y;
        }

        internal GeoJSON.Text.Geometry.LineString ToGeoJson(Func<TerrainPoint, GeoJSON.Text.Geometry.IPosition> project)
        {
            return new GeoJSON.Text.Geometry.LineString(Points.Select(project).ToList());
        }

        public float Distance(TerrainPoint p)
        {
            return (float)AsLineString.Distance(new Point(p.X, p.Y));
        }

        public TerrainPoint NearestPointBoundary(TerrainPoint p)
        {
            var distance = new DistanceOp(AsLineString, new Point(p.X, p.Y));
            var segment = distance.NearestPoints();
            return new TerrainPoint((float)segment[0].X, (float)segment[0].Y);
        }

        public TerrainPath PreventSplines(float threshold)
        {
            return new TerrainPath(GeometryHelper.PointsOnPath(PreventSplines(Points, threshold), threshold * 10));
        }

        public static List<TerrainPoint> PreventSplines(List<TerrainPoint> source, float threshold)
        {
            if (source.Count <= 2)
            {
                return source;
            }
            var points = new List<TerrainPoint>() { source[0], source[1] };
            foreach (var point in source.Skip(2))
            {
                // A -> B -> C
                var prevPoint = points[points.Count - 1];
                var next = point.Vector - prevPoint.Vector; // C - B
                var prev = prevPoint.Vector - points[points.Count - 2].Vector; // B - A
                var nextImpacted = next.Length() > threshold;
                var prevImpacted = prev.Length() > threshold;
                if (nextImpacted || prevImpacted)
                {
                    var angle = Math.Abs(Math.Atan2(next.Y, next.X) - Math.Atan2(prev.Y, prev.X));
                    if (angle > Math.PI / 4)
                    {
                        if (prevImpacted)
                        {
                            points.RemoveAt(points.Count - 1);
                            points.Add(prevPoint - (prev * threshold / prev.Length()));
                            points.Add(prevPoint);
                        }
                        if (nextImpacted)
                        {
                            points.Add(prevPoint + (next * threshold / next.Length()));
                        }
                    }
                }
                points.Add(point);
            }
            return points;
        }

        public TerrainPath ExtendBothEnds(float extendAtEachEnd)
        {
            var newStart = Points[0] + Vector2.Normalize(Points[0].Vector - Points[1].Vector) * extendAtEachEnd;
            var newEnd = Points[Points.Count - 1] + Vector2.Normalize(Points[Points.Count - 1].Vector - Points[Points.Count - 2].Vector) * extendAtEachEnd;
            var points = Points.ToList();
            points[0] = newStart;
            points[Points.Count - 1] = newEnd;
            return new TerrainPath(points);
        }

        public static TerrainPath FromRectangle(TerrainPoint start, TerrainPoint end)
        {
            return new TerrainPath(
                new List<TerrainPoint>()
                {
                    start,
                    new TerrainPoint(end.X, start.Y),
                    end,
                    new TerrainPoint(start.X, end.Y),
                    start
                });
        }

        public static TerrainPath FromCircle(TerrainPoint origin, float radius)
        {
            return new TerrainPath(
                GeometryHelper.SimpleCircle(origin.Vector, radius).Select(v => new TerrainPoint(v)).ToList());
        }

        public Vector2 GetNormalizedVectorAtIndex(int index)
        {
            var a = Points[Math.Max(index - 1, 0)];
            var b = Points[Math.Min(index + 1, Points.Count - 1)];
            return Vector2.Normalize(b.Vector - a.Vector);
        }

        public IEnumerable<TerrainPath> ClippedKeepOrientation(TerrainPolygon polygon)
        {
            if (!EnveloppeIntersects(polygon))
            {
                return Enumerable.Empty<TerrainPath>();
            }
            return KeepOrientation(Intersection(polygon));
        }

        private List<TerrainPath> KeepOrientation(List<TerrainPath> clipped)
        {
            var initialPoints = Points.Select(p => p.ToIntPointPrecision()).ToList();
            foreach (var result in clipped)
            {
                if (result.Points.Count < 2)
                {
                    continue;
                }
                var indices = result.Points.Select(np => initialPoints.FindIndex(p => TerrainPoint.Equals(np, p))).Where(i => i != -1).Take(2).ToList();
                if (indices.Count > 1)
                {
                    if (indices[0] > indices[1])
                    {
                        result.Points.Reverse();
                    }
                }
                else if (indices.Count == 1)
                {
                    var initialReferenceIndex = indices[0];
                    var sharedReferencePoint = initialPoints[initialReferenceIndex];
                    var resultReferenceIndex = result.Points.FindIndex(p => TerrainPoint.Equals(sharedReferencePoint, p));

                    if (resultReferenceIndex == result.Points.Count - 1)
                    {
                        if (initialReferenceIndex == 0)
                        {
                            result.Points.Reverse();
                        }
                        else
                        {
                            CheckVector(result, sharedReferencePoint, result.Points[resultReferenceIndex - 1], initialPoints[initialReferenceIndex - 1]);
                        }
                    }
                    else
                    {
                        if (initialReferenceIndex == initialPoints.Count - 1)
                        {
                            result.Points.Reverse();
                        }
                        else
                        {
                            CheckVector(result, sharedReferencePoint, result.Points[resultReferenceIndex + 1], initialPoints[initialReferenceIndex + 1]);
                        }
                    }
                }
#if DEBUG
                else
                {
                    throw new InvalidOperationException("Unsupported edge case");
                }
#endif
            }
            return clipped;
        }

        private static void CheckVector(TerrainPath result, TerrainPoint sharedReferencePoint, TerrainPoint resultComparePoint, TerrainPoint initialComparePoint)
        {
            var initialVect = Vector2.Normalize(initialComparePoint.Vector - sharedReferencePoint.Vector);
            var resultVect = Vector2.Normalize(resultComparePoint.Vector - sharedReferencePoint.Vector);
            if (Vector2.Dot(initialVect, resultVect) < 0)
            {
                result.Points.Reverse();
            }
        }



        public bool IsClosed => FirstPoint.Equals(LastPoint);

        public bool IsCounterClockWise => Points.IsCounterClockWise();

        public bool IsClockWise => !Points.IsCounterClockWise();

        public TerrainPath Reversed()
        {
            return new TerrainPath(Enumerable.Reverse(Points).ToList());
        }

        public TerrainPolygon ToPolygon()
        {
            if (!IsClosed)
            {
                return new TerrainPolygon(Points.Concat(new[] { FirstPoint }).ToList());
            }
            return new TerrainPolygon(Points);
        }

        public static List<TerrainPoint> Simplify(IReadOnlyList<TerrainPoint> input)
        {
            if (input.Count < 3)
            {
                return input.ToList();
            }
            var previousPoint = input[0];
            var result = new List<TerrainPoint>() { previousPoint };
            for (int i = 1; i < input.Count - 1; i++)
            {
                var thisPoint = input[i];
                var nextPoint = input[i + 1];
                var extrapolated = Vector2.Lerp(previousPoint.Vector, nextPoint.Vector, (thisPoint.Vector - previousPoint.Vector).Length() / (nextPoint.Vector - previousPoint.Vector).Length());
                if ((extrapolated - thisPoint.Vector).Length() > 0.5)
                {
                    result.Add(thisPoint);
                    previousPoint = thisPoint;
                }
            }
            result.Add(input[input.Count - 1]);
            return result;
        }
    }
}
