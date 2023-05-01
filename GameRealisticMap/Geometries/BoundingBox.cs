using System.Numerics;
using System.Text.Json.Serialization;
using GeoAPI.Geometries;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Geometries
{
    public class BoundingBox : IBoundingShape
    {
        private readonly Lazy<TerrainPolygon> terrainPolygon;

        public BoundingBox(float cx, float cy, float cw, float ch, float ca, TerrainPoint[] points)
            : this(new TerrainPoint(cx, cy), cw, ch, ca, points)
        {
        }

        public BoundingBox(TerrainPoint center, float cw, float ch, float ca, TerrainPoint[] points)
        {
            Center = center;
            Width = cw;
            Height = ch;
            Angle = ca;
            Points = points;
            terrainPolygon = new Lazy<TerrainPolygon>(() => new TerrainPolygon(Points.Concat(Points.Take(1)).ToList(), TerrainPolygon.NoHoles));
        }

        [JsonConstructor]
        public BoundingBox(TerrainPoint center, float width, float height, float angle)
            : this(center, width, height, angle,
                  GeometryHelper.RotatedRectangleDegrees(center.Vector, new Vector2(width, height), angle)
                  .Select(v => new TerrainPoint(v))
                  .ToArray())
        {

        }

        public TerrainPoint Center { get; }

        /// <summary>
        /// Width in meters
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Heigth in meters
        /// </summary>
        public float Height { get; }

        /// <summary>
        /// Angle in degrees (trigonometric / anti-clockwise)
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// Heading in degrees (clockwise)
        /// </summary>
        [JsonIgnore]
        public float Heading => -Angle;

        /// <summary>
        /// Points of rectangle
        /// </summary>
        [JsonIgnore]
        public TerrainPoint[] Points { get; }

        [JsonIgnore]
        public Vector2 Size => new Vector2(Width, Height);

        [JsonIgnore]
        public IPolygon Poly => terrainPolygon.Value.AsPolygon;

        [JsonIgnore]
        public TerrainPolygon Polygon => terrainPolygon.Value;

        [JsonIgnore]
        public TerrainPoint MinPoint => new TerrainPoint(Points.Min(p => p.X), Points.Min(p => p.Y));

        [JsonIgnore]
        public TerrainPoint MaxPoint => new TerrainPoint(Points.Max(p => p.X), Points.Max(p => p.Y));

        [JsonIgnore]
        public float Surface => Width * Height;

        public BoundingBox Add(BoundingBox other)
        {
            return Compute(Points.Concat(other.Points).ToArray());
        }

        public BoundingBox RotateM90()
        {
            return new BoundingBox(Center, Height, Width, Angle - 90, Points);
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"({Center.X};{Center.Y}) ({Width}x{Height}) {Angle}°");
        }

        //public bool MayIntersects(BoundingBox other)
        //{
        //    var delta = Math.Max(Width, Height) + Math.Max(other.Width, other.Height);
        //    return Math.Pow(other.Center.X - Center.X, 2) + Math.Pow(other.Center.Y - Center.Y, 2) < Math.Pow(delta, 2);
        //}

        public static BoundingBox Compute(TerrainPoint[] points)
        {
            int i;

            double ax;
            double ay;
            double bx;
            double by;

            double dx;
            double dy;

            double minx;
            double miny;
            double maxx;
            double maxy;

            double theta;
            double minarea;
            double area;

            double cw = 0;
            double ch = 0;
            double cx = 0;
            double cy = 0;
            double ca = 0;

            double c1xp = 0;
            double c2xp = 0;
            double c3xp = 0;
            double c4xp = 0;
            double c1yp = 0;
            double c2yp = 0;
            double c3yp = 0;
            double c4yp = 0;

            int j;

            minarea = double.MaxValue;

            ax = points[points.Length - 1].X;
            ay = points[points.Length - 1].Y;

            for (i = 0; i < points.Length; i++)
            {
                bx = points[i].X; by = points[i].Y;
                dx = (ax - bx);
                dy = (ay - by);
                theta = Math.Atan2(dy, dx);
                maxx = double.MinValue;
                maxy = double.MinValue;
                minx = double.MaxValue;
                miny = double.MaxValue;
                for (j = 0; j < points.Length; j++)
                {
                    var rx = ((points[j].X - ax) * Math.Cos(theta)) + ((points[j].Y - ay) * Math.Sin(theta));
                    var ry = ((points[j].Y - ay) * Math.Cos(theta)) - ((points[j].X - ax) * Math.Sin(theta));
                    if (rx > maxx) { maxx = rx; }
                    if (ry > maxy) { maxy = ry; }
                    if (rx < minx) { minx = rx; }
                    if (ry < miny) { miny = ry; }
                }
                area = (maxx - minx) * (maxy - miny);
                if (area < minarea)
                {
                    minarea = area;
                    cw = (maxx - minx);
                    ch = (maxy - miny);
                    ca = theta * 180.0 / Math.PI;

                    var cosMTheta = Math.Cos(-theta);
                    var sinMTheta = Math.Sin(-theta);

                    cx = ((minx + maxx) * cosMTheta / 2) + ((miny + maxy) * sinMTheta / 2) + ax;
                    cy = ((miny + maxy) * cosMTheta / 2) - ((minx + maxx) * sinMTheta / 2) + ay;

                    c1xp = (minx * cosMTheta) + (miny * sinMTheta) + ax;
                    c1yp = (miny * cosMTheta) - (minx * sinMTheta) + ay;
                    c2xp = (maxx * cosMTheta) + (miny * sinMTheta) + ax;
                    c2yp = (miny * cosMTheta) - (maxx * sinMTheta) + ay;
                    c3xp = (maxx * cosMTheta) + (maxy * sinMTheta) + ax;
                    c3yp = (maxy * cosMTheta) - (maxx * sinMTheta) + ay;
                    c4xp = (minx * cosMTheta) + (maxy * sinMTheta) + ax;
                    c4yp = (maxy * cosMTheta) - (minx * sinMTheta) + ay;

                    //cx = (c3xp + c1xp) / 2;
                    //cy = (c3yp + c1yp) / 2;
                }
                ax = bx;
                ay = by;
            }

            return new BoundingBox((float)cx, (float)cy, (float)cw, (float)ch, (float)ca, new[] {
                new TerrainPoint((float)c1xp, (float)c1yp),
                new TerrainPoint((float)c2xp, (float)c2yp),
                new TerrainPoint((float)c3xp, (float)c3yp),
                new TerrainPoint((float)c4xp, (float)c4yp)});
        }


        private static (TerrainPoint? Point, float Distance) ClosestIntersection(IEnumerable<(TerrainPoint First, TerrainPoint Second)> segments, TerrainPoint start, TerrainPoint end)
        {
            TerrainPoint? result = null;
            float resultFromStart = float.PositiveInfinity;
            foreach(var segment in segments)
            {
                Vector2 intersection;
                if (VectorHelper.HasIntersection(start.Vector, end.Vector, segment.First.Vector, segment.Second.Vector, out intersection))
                {
                    var intersectionFromStart = (start.Vector - intersection).Length();
                    if (intersectionFromStart != 0 && (result == null || intersectionFromStart < resultFromStart))
                    {
                        result = new TerrainPoint(intersection);
                        resultFromStart = intersectionFromStart;
                    }
                }
            }
            return (result, resultFromStart);
        }

        class InnerCandidate
        {
            public InnerCandidate(TerrainPoint a, TerrainPoint b, TerrainPoint c)
            {
                var ab = (b.Vector - a.Vector);
                var ac = (c.Vector - a.Vector);
                Width = ab.Length();
                Height = ac.Length();
                Center = a + ((ab + ac) / 2);
                Angle = MathF.Atan2(-ab.Y, -ab.X) * 180 / MathF.PI;
            }

            public float Width { get; }
            public float Height { get; }
            public TerrainPoint Center { get; }
            public float Angle { get; }
            public float Area => Width * Height;

            internal BoundingBox ToBoundingBox()
            {
                return new BoundingBox(Center, Width, Height, Angle);
            }
        }

        public static BoundingBox? ComputeInner(IEnumerable<TerrainPoint> points)
        {
            var outerSegments = points.Zip(points.Skip(1).Concat(points.Take(1))).ToList();
            var allSegments = points.SelectMany(p1 => points.Where(p2 => !TerrainPoint.Equals(p2, p1)).Select(p2 => new { P1 = p1, P2 = p2 })).ToList();
            InnerCandidate? result = null;
            foreach (var segment in allSegments)
            {
                TerrainPoint? xp1;
                TerrainPoint? xp2;

                var candidate = Consider(outerSegments, segment.P1, segment.P2, out xp1, out xp2);

                if (candidate == null)
                {
                    if (xp1 != null)
                    {
                        for (var i = 99; i > 0 && candidate == null; i--)
                        {
                            var p2 = new TerrainPoint(Vector2.Lerp(segment.P1.Vector, segment.P2.Vector, i / 100f));
                            candidate = Consider(outerSegments, segment.P1, p2, out _, out _);
                        }
                    }
                    else if (xp2 != null)
                    {
                        for (var i = 1; i < 100 && candidate == null; i++)
                        {
                            var p1 = new TerrainPoint(Vector2.Lerp(segment.P1.Vector, segment.P2.Vector, i / 100f));
                            candidate = Consider(outerSegments, p1, segment.P2, out _, out _);
                        }
                    }
                }
                 
                if (candidate != null && (result == null || candidate.Area > result.Area))
                {
                    result = candidate;
                }
            }
            return result?.ToBoundingBox();
        }

        private static InnerCandidate? Consider(List<(TerrainPoint First, TerrainPoint Second)> outerSegments, TerrainPoint p1, TerrainPoint p2, out TerrainPoint? xp1, out TerrainPoint? xp2)
        {
            var delta = Vector2.Normalize(p2.Vector - p1.Vector);
            var normal = Vector2.Transform(delta, GeometryHelper.Rotate90);
            var x1 = ClosestIntersection(outerSegments, p1, p1 + normal * 1000f);
            var x2 = ClosestIntersection(outerSegments, p2, p2 + normal * 1000f);
            xp1 = x1.Point;
            xp2 = x2.Point;
            if (x1.Point != null && x2.Point != null)
            {
                return (x1.Distance <= x2.Distance) ?
                    new InnerCandidate(p1, p2, x1.Point) :
                    new InnerCandidate(p2, p1, x2.Point);
            }
            return null;
        }
    }

}