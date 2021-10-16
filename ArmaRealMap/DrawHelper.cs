using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArmaRealMap.Geometries;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    internal static class DrawHelper
    {
        internal static void FillPolygonWithHoles(Image<Rgb24> img, List<PointF> outer, List<List<PointF>> holes, IBrush brush, ShapeGraphicsOptions shapeGraphicsOptions)
        {
            FillPolygonWithHoles<Rgb24, Rgba32>(img, outer, holes, brush, Color.Transparent, shapeGraphicsOptions);
        }

        internal static void FillPolygonWithHoles(Image<Rgba32> img, List<PointF> outer, List<List<PointF>> holes, IBrush brush, ShapeGraphicsOptions shapeGraphicsOptions)
        {
            FillPolygonWithHoles<Rgba32, Rgba32>(img, outer, holes, brush, Color.Transparent, shapeGraphicsOptions);
        }

        internal static void FillPolygonWithHoles<TPixel,TPixelAlpha>(Image<TPixel> img, List<PointF> outer, List<List<PointF>> holes, IBrush brush, TPixelAlpha transparent, ShapeGraphicsOptions shapeGraphicsOptions)
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
                        var xor = new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { AlphaCompositionMode = PixelAlphaCompositionMode.Xor, Antialias = shapeGraphicsOptions.GraphicsOptions.Antialias } };
                        p.FillPolygon(shapeGraphicsOptions, brush, outer.Select(p => new PointF(p.X - clip.X, p.Y - clip.Y)).ToArray());
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
                img.Mutate(p => p.FillPolygon(shapeGraphicsOptions, brush, outer.ToArray()));
            }
        }

        internal static void FillGeometry(Image<Rgb24> img, IBrush solidBrush, Geometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<PointF>> toPixels)
        {
            FillGeometry<Rgb24, Rgba32>(img, solidBrush, geometry, toPixels, Color.Transparent, new ShapeGraphicsOptions());
        }

        internal static void FillGeometry(Image<Rgba32> img, IBrush solidBrush, Geometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<PointF>> toPixels)
        {
            FillGeometry<Rgba32, Rgba32>(img, solidBrush, geometry, toPixels, Color.Transparent, new ShapeGraphicsOptions());
        }

        internal static void FillGeometry<TPixel, TPixelAlpha>(Image<TPixel> img, IBrush solidBrush, Geometry geometry, Func<IEnumerable<Coordinate>, IEnumerable<PointF>> toPixels, TPixelAlpha transparent, ShapeGraphicsOptions shapeGraphicsOptions)
            where TPixel : unmanaged, IPixel<TPixel>
            where TPixelAlpha : unmanaged, IPixel<TPixelAlpha>
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                foreach (var geo in ((GeometryCollection)geometry).Geometries)
                {
                    FillGeometry(img, solidBrush, geo, toPixels, transparent,
                        shapeGraphicsOptions);
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
                        solidBrush,
                        transparent,
                        shapeGraphicsOptions);
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

        internal static void DrawPath(IImageProcessingContext d, TerrainPath path, float width, SolidBrush brush, MapInfos map, bool antiAlias = true)
        {
            DrawPath(d, path.Points, width, brush, map.TerrainToPixelsPoint, new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = antiAlias } });
        }

        internal static void DrawPath<T>(IImageProcessingContext d, IEnumerable<T> points, float width, SolidBrush brush, Func<T, PointF> project, ShapeGraphicsOptions shapeGraphicsOptions)
        {
            var pixelsPoints = points.Select(project).ToArray();
            d.DrawLines(shapeGraphicsOptions, brush, width, pixelsPoints);
        }

        internal static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, SolidBrush brush, MapInfos map)
        {
            FillPolygonWithHoles(d, polygon.Shell.Select(map.TerrainToPixelsPoint), polygon.Holes.Select(map.TerrainToPixelsPoints).ToList(), brush);
        }

        internal static void FillPolygonWithHoles(IImageProcessingContext p, IEnumerable<PointF> outer, List<IEnumerable<PointF>> holes, IBrush brush)
        {
            if (holes.Any())
            {
                var clip = new Rectangle(
                    (int)outer.Min(p => p.X) - 1,
                    (int)outer.Min(p => p.Y) - 1,
                    (int)(outer.Max(p => p.X) - outer.Min(p => p.X)) + 2,
                    (int)(outer.Max(p => p.Y) - outer.Min(p => p.Y)) + 2);
                using (var dimg = new Image<Rgba32>(clip.Width, clip.Height, Color.Transparent))
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
                    try
                    {
                        p.DrawImage(dimg, clip.Location, 1);
                    }
                    catch
                    {

                    }
                }
            }
            else
            {
                p.FillPolygon(brush, outer.ToArray());
            }
        }

        internal const int Chunk = 10240;

        internal static int SavePngChuncked(Image<Rgb24> img, string filename)
        {
            //img.Save(filename); --> Too expensive
            if (img.Width > Chunk) 
            {
                // terrain builder as a 32 bits process, does not like too large images. So make chunks of image
                // => 20480x20480 takes 3 hours to import, 10240x10240 takes 10 minutes, so 40 minutes vs 180 !
                int num = 2;
                while(img.Width / num > Chunk)
                {
                    num = num * 2;
                }
                var report = new ProgressReport("PngChuncked", num * num);
                int chunkSize = img.Width / num;
                var chunk = new Image<Rgb24>(chunkSize, chunkSize);
                for(int x = 0; x < num; x ++)
                {
                    for (int y = 0; y < num; y++)
                    {
                        var pos = new SixLabors.ImageSharp.Point(-x * chunkSize, -y * chunkSize);
                        chunk.Mutate(c => c.DrawImage(img, pos, 1.0f));
                        chunk.Save(System.IO.Path.ChangeExtension(filename, $"{x}_{y}.png"));
                        report.ReportOneDone();
                    }
                }
                report.TaskDone();
                return num;
            }
            else
            {
                img.Save(System.IO.Path.ChangeExtension(filename, $"0_0.png"));
            }
            return 1;
        }
    }
}
