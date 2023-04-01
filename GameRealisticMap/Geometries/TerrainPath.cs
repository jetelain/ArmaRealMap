using System.Numerics;
using ClipperLib;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace GameRealisticMap.Geometries
{
    public class TerrainPath : ITerrainGeometry
    {
        private readonly Lazy<LineString> asLineString;

        public TerrainPath(params TerrainPoint[] points)
            : this(points.ToList())
        {
        }

        public TerrainPath(List<TerrainPoint> points)
        {
            this.Points = points;
            MinPoint = new TerrainPoint(points.Min(p => p.X), points.Min(p => p.Y));
            MaxPoint = new TerrainPoint(points.Max(p => p.X), points.Max(p => p.Y));
            EnveloppeSize = MaxPoint.Vector - MinPoint.Vector;
            asLineString = new Lazy<LineString>(() => ToLineString(c => new Coordinate(c.X, c.Y)));
        }

        public List<TerrainPoint> Points { get; }
        public TerrainPoint FirstPoint => Points[0];
        public TerrainPoint LastPoint => Points[Points.Count-1];

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        public Vector2 EnveloppeSize { get; }

        public LineString AsLineString => asLineString.Value;

        public float Length 
        { 
            get
            {
                // TODO: Own algorithm
                return (float)AsLineString.Length;
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
        public bool EnveloppeIntersects(ITerrainGeometry other)
        {
            return other.MinPoint.X <= MaxPoint.X &&
                other.MinPoint.Y <= MaxPoint.Y &&
                other.MaxPoint.X >= MinPoint.X &&
                other.MaxPoint.Y >= MinPoint.Y;
        }
    }
}
