using System;
using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    public static class DrawHelper
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

        internal static void DrawPath(IImageProcessingContext d, TerrainPath path, float width, SolidBrush brush, MapInfos map, bool antiAlias = true)
        {
            DrawPath(d, path.Points, width, brush, map.TerrainToPixelsPoint, new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = antiAlias } });
        }

        internal static void DrawPath<T>(IImageProcessingContext d, IEnumerable<T> points, float width, SolidBrush brush, Func<T, PointF> project, ShapeGraphicsOptions shapeGraphicsOptions)
        {
            var pixelsPoints = points.Select(project).ToArray();
            d.DrawLines(shapeGraphicsOptions, brush, width, pixelsPoints);
        }

        internal static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, MapInfos map, bool antiAlias = true)
        {
            DrawPolygon(d, polygon, brush,map, new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = antiAlias } });
        }

        internal static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, MapInfos map, ShapeGraphicsOptions shapeGraphicsOptions)
        {
            FillPolygonWithHoles(d, polygon.Shell.Select(map.TerrainToPixelsPoint), polygon.Holes.Select(map.TerrainToPixelsPoints).ToList(), brush, shapeGraphicsOptions);
        }
        public static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, Func<IEnumerable<TerrainPoint>, IEnumerable<PointF>> toPixels)
        {
            FillPolygonWithHoles(d, toPixels(polygon.Shell), polygon.Holes.Select(toPixels).ToList(), brush, new ShapeGraphicsOptions());
        }

        public static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, Func<IEnumerable<TerrainPoint>, IEnumerable<PointF>> toPixels, ShapeGraphicsOptions shapeGraphicsOptions)
        {
            FillPolygonWithHoles(d, toPixels(polygon.Shell), polygon.Holes.Select(toPixels).ToList(), brush, shapeGraphicsOptions);
        }

        internal static void FillPolygonWithHoles(IImageProcessingContext p, IEnumerable<PointF> outer, List<IEnumerable<PointF>> holes, IBrush brush, ShapeGraphicsOptions shapeGraphicsOptions)
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
                        var xor = new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { AlphaCompositionMode = PixelAlphaCompositionMode.Xor, Antialias = shapeGraphicsOptions.GraphicsOptions.Antialias } };
                        p.FillPolygon(shapeGraphicsOptions, brush, outer.Select(p => new PointF(p.X - clip.X, p.Y - clip.Y)).ToArray());
                        foreach (var hpoints in holes)
                        {
                            p.FillPolygon(xor, brush, hpoints.Select(p => new PointF(p.X - clip.X, p.Y - clip.Y)).ToArray());
                        }
                    });
                    try
                    {
                        p.DrawImage(dimg, clip.Location, 1);
                    }
                    catch(ImageProcessingException)
                    {

                    }
                }
            }
            else
            {
                try
                {
                    p.FillPolygon(shapeGraphicsOptions, brush, outer.ToArray());
                }
                catch(ImageProcessingException)
                {

                }
            }
        }

        internal static void DrawPolygonEdges(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, MapInfos map, float width, bool antiAlias = true)
        {
            DrawPolygonEdges(d, polygon, brush, map, new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { Antialias = antiAlias } }, width);
        }

        internal static void DrawPolygonEdges(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, MapInfos map, ShapeGraphicsOptions shapeGraphicsOptions, float width)
        {
            DrawPolygonEdgesWithHoles(d, polygon.Shell.Select(map.TerrainToPixelsPoint), polygon.Holes.Select(map.TerrainToPixelsPoints).ToList(), brush, shapeGraphicsOptions, width);
        }

        internal static void DrawPolygonEdgesWithHoles(IImageProcessingContext p, IEnumerable<PointF> outer, List<IEnumerable<PointF>> holes, IBrush brush, ShapeGraphicsOptions shapeGraphicsOptions, float width)
        {
            p.DrawPolygon(shapeGraphicsOptions, brush, width, outer.ToArray());

            foreach (var hpoints in holes)
            {
                p.DrawPolygon(shapeGraphicsOptions, brush, width, hpoints.ToArray());
            }
        }

        internal const int Chunk = 10240;

        internal static int SavePngChuncked<TPixel>(Image<TPixel> img, string filename) where TPixel : unmanaged, IPixel<TPixel>
        {
            img.Save(filename);

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
                var chunk = new Image<TPixel>(chunkSize, chunkSize);
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
