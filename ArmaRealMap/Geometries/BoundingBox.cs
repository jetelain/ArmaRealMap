using System;
using SixLabors.ImageSharp;

namespace ArmaRealMap.Geometries
{
    internal class BoundingBox
    {
        public BoundingBox(float cx, float cy, float cw, float ch, float ca)
        {
            Center = new PointF(cx, cy);
            Width = cw;
            Height = ch;
            Angle = ca;
        }

        public PointF Center { get; }

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


        internal static BoundingBox Compute(PointF[] points)
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
                    cx = ((minx + maxx) * Math.Cos(-theta) / 2) + ((miny + maxy) * Math.Sin(-theta) / 2) + ax;
                    cy = ((miny + maxy) * Math.Cos(-theta) / 2) - ((minx + maxx) * Math.Sin(-theta) / 2) + ay;

                    //var c1xp = (minx * Math.Cos(-theta)) + (miny * Math.Sin(-theta)) + ax;
                    //var c1yp = (miny * Math.Cos(-theta)) - (minx * Math.Sin(-theta)) + ay;
                    //var c2xp = (maxx * Math.Cos(-theta)) + (miny * Math.Sin(-theta)) + ax;
                    //var c2yp = (miny * Math.Cos(-theta)) - (maxx * Math.Sin(-theta)) + ay;
                    //var c3xp = (maxx * Math.Cos(-theta)) + (maxy * Math.Sin(-theta)) + ax;
                    //var c3yp = (maxy * Math.Cos(-theta)) - (maxx * Math.Sin(-theta)) + ay;
                    //var c4xp = (minx * Math.Cos(-theta)) + (maxy * Math.Sin(-theta)) + ax;
                    //var c4yp = (maxy * Math.Cos(-theta)) - (minx * Math.Sin(-theta)) + ay;
                    //cx = (c3xp + c1xp) / 2;
                    //cy = (c3yp + c1yp) / 2;
                }
                ax = bx;
                ay = by;
            }

            return new BoundingBox((float)cx, (float)cy, (float)cw, (float)ch, (float)ca);
        }
    }
}