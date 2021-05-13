using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    internal static class DrawHelper
    {
        internal static void FillPolygonWithHoles(Image<Rgb24> img, List<PointF> outer, List<List<PointF>> holes, IBrush brush)
        {
            FillPolygonWithHoles<Rgb24, Rgba32>(img, outer, holes, brush, Color.Transparent);
        }

        internal static void FillPolygonWithHoles(Image<Rgba32> img, List<PointF> outer, List<List<PointF>> holes, IBrush brush)
        {
            FillPolygonWithHoles<Rgba32, Rgba32>(img, outer, holes, brush, Color.Transparent);
        }

        internal static void FillPolygonWithHoles<TPixel,TPixelAlpha>(Image<TPixel> img, List<PointF> outer, List<List<PointF>> holes, IBrush brush, TPixelAlpha transparent)
            where TPixel : unmanaged, IPixel<TPixel>
            where TPixelAlpha : unmanaged, IPixel<TPixelAlpha>
        {
            if (holes.Any())
            {
                var clip = new Rectangle(
                    (int)outer.Min(p => p.X) - 1,
                    (int)outer.Min(p => p.Y) - 1,
                    (int)(outer.Max(p => p.X) - outer.Min(p => p.X)) + 2,
                    (int)(outer.Max(p => p.Y) - outer.Min(p => p.Y)) + 2);
                using (var dimg = new Image<TPixelAlpha>(clip.Width, clip.Height, transparent))
                {
                    dimg.Mutate(p =>
                    {
                        var xor = new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { AlphaCompositionMode = PixelAlphaCompositionMode.Xor } };
                        p.FillPolygon(brush, outer.Select(p => new PointF(p.X - clip.X, p.Y - clip.Y)).ToArray());
                        foreach (var hpoints in holes)
                        {
                            p.FillPolygon(xor, brush, hpoints.Select(p => new PointF(p.X - clip.X, p.Y - clip.Y)).ToArray());
                        }
                    });
                    img.Mutate(p => p.DrawImage(dimg, clip.Location, 1));
                }
            }
            else
            {
                img.Mutate(p => p.FillPolygon(brush, outer.ToArray()));
            }
        }

        internal static void FillGeometry(Image<Rgb24> img, IBrush solidBrush, Geometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<PointF>> toPixels)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                foreach (var geo in ((GeometryCollection)geometry).Geometries)
                {
                    FillGeometry(img, solidBrush, geo, toPixels);
                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (Polygon)geometry;
                try
                {
                    FillPolygonWithHoles(img,
                        toPixels(poly.Shell.Coordinates).ToList(),
                        poly.Holes.Select(h => toPixels(h.Coordinates).Select(p => new PointF(p.X, p.Y)).ToList()).ToList(),
                        solidBrush);
                }
                catch
                {

                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (LineString)geometry;
                var points = toPixels(line.Coordinates).ToArray();
                try
                {
                    if (line.IsClosed)
                    {
                        img.Mutate(p => p.FillPolygon(solidBrush, points));
                    }
                    else
                    {
                        img.Mutate(p => p.DrawLines(solidBrush, 6.0f, points));
                    }
                }
                catch
                {

                }
            }
            else
            {
                throw new NotSupportedException(geometry.OgcGeometryType.ToString());
            }
        }
    }
}
