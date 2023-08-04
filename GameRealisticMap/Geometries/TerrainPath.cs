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

        public List<TerrainPoint> Points { get; }

        [JsonIgnore]
        public TerrainPoint FirstPoint => Points[0];

        [JsonIgnore]
        public TerrainPoint LastPoint => Points[Points.Count-1];

        [JsonIgnore]
        public TerrainPoint MinPoint { get; }

        [JsonIgnore]
        public TerrainPoint MaxPoint { get; }

        [JsonIgnore]
        public Vector2 EnveloppeSize { get; }

        [JsonIgnore]
        public LineString AsLineString => asLineString.Value;

        [JsonIgnore]
        public float Length 
        { 
            get
            {
                var length = 0f;
                var prev = FirstPoint;
                TerrainPoint point;
                for(int i = 1; i < Points.Count; ++i)
                {
                    point = Points[i];
                    length += (point.Vector - prev.Vector).Length();
                    prev = point;
                }
                return length;
            }
        }

        public LineString ToLineString(Func<TerrainPoint, Coordinate> project)
        {
            return new LineString(Points.Select(project).ToArray());
        }

        public IEnumerable<TerrainPolygon> ToTerrainPolygon(float width)
        {
            return TerrainPolygon.FromPath(Points, width);
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

        public List<TerrainPath> Intersection(TerrainPolygon polygon)
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

        public IEnumerable<TerrainPath> ClippedBy(TerrainPolygon polygon)
        {
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
            var newEnd = Points[Points.Count-1] + Vector2.Normalize(Points[Points.Count - 1].Vector - Points[Points.Count - 2].Vector) * extendAtEachEnd;
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

        internal IEnumerable<TerrainPath> ClippedKeepOrientation(TerrainPolygon polygon)
        {
            var intPointPrecision = Points.Select(p => p.ToIntPointPrecision()).ToList();
            var intPointFirst = intPointPrecision[0];

            var clipped = Intersection(polygon);

            foreach(var result in clipped)
            {
                if (!TerrainPoint.Equals(result.FirstPoint, intPointFirst))
                {
                    if (TerrainPoint.Equals(result.LastPoint, intPointFirst)) // trivial case : path was not really changed, only reverse
                    {
                        result.Points.Reverse();
                    }
                    else if (result.Points.Count > 3)
                    {
                        var coreItems = result.Points.GetRange(1, result.Points.Count - 2);
                        var index1 = intPointPrecision.FindIndex(p => TerrainPoint.Equals(coreItems[0], p));
                        var index2 = intPointPrecision.FindIndex(p => TerrainPoint.Equals(coreItems[1], p));
                        if (index1 > index2)
                        {
                            result.Points.Reverse();
                        }
                    }
                    else
                    {
                        // Find an alternate method
                    }
                }
            }
            return clipped;
        }

        public bool IsClosed => FirstPoint.Equals(LastPoint);

        public bool IsCounterClockWise => Points.IsCounterClockWise();
    }
}
