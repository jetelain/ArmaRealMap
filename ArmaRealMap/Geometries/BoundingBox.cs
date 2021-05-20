using System;
using System.Linq;
using System.Numerics;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;

namespace ArmaRealMap.Geometries
{
    internal class BoundingBox : IBoundingShape
    {
        private readonly Lazy<Polygon> polygon;

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
            polygon = new Lazy<Polygon>(() => new Polygon(new LinearRing(Points.Concat(Points.Take(1)).Select(p => new Coordinate(p.X, p.Y)).ToArray())));
        }

        public BoundingBox(BoxJson json)
            : this(new TerrainPoint(json.Center[0], json.Center[1]), json.Width, json.Height, json.Angle, json.Points.Select(p => new TerrainPoint(p[0],p[1])).ToArray())
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
        /// Angle in degrees
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// Points of rectangle
        /// </summary>
        public TerrainPoint[] Points { get; }

        public Vector2 Size => new Vector2(Width, Height);

        public Polygon Poly => polygon.Value;

        public TerrainPoint MinPoint => new TerrainPoint(Points.Min(p => p.X), Points.Min(p => p.Y));

        public TerrainPoint MaxPoint => new TerrainPoint(Points.Max(p => p.X), Points.Max(p => p.Y));

        public BoundingBox Add (BoundingBox other)
        {
            return Compute(Points.Concat(other.Points).ToArray());
        }

        public BoundingBox RotateM90()
        {
            return new BoundingBox(Center, Height, Width, Angle - 90, Points);
        }

        //public bool MayIntersects(BoundingBox other)
        //{
        //    var delta = Math.Max(Width, Height) + Math.Max(other.Width, other.Height);
        //    return Math.Pow(other.Center.X - Center.X, 2) + Math.Pow(other.Center.Y - Center.Y, 2) < Math.Pow(delta, 2);
        //}

        public BoxJson ToJson()
        {
            return new BoxJson()
            {
                Center = new[] { Center.X, Center.Y },
                Width = Width,
                Height = Height,
                Points = Points.Select(p => new[] { p.X, p.Y }).ToArray(),
                Angle = Angle
            };
        }

        internal static BoundingBox Compute(TerrainPoint[] points)
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
    }
}