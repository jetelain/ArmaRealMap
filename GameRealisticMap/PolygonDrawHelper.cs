using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap
{
    public static class PolygonDrawHelper
    {
        public static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, Func<IEnumerable<TerrainPoint>, IEnumerable<PointF>> toPixels)
        {
            FillPolygonWithHoles(d, toPixels(polygon.Shell), polygon.Holes.Select(toPixels).ToList(), brush, new DrawingOptions());
        }

        public static void DrawPolygon(IImageProcessingContext d, TerrainPolygon polygon, IBrush brush, DrawingOptions drawingOptions, Func<IEnumerable<TerrainPoint>, IEnumerable<PointF>> toPixels)
        {
            FillPolygonWithHoles(d, toPixels(polygon.Shell), polygon.Holes.Select(toPixels).ToList(), brush, drawingOptions);
        }

        private static void FillPolygonWithHoles(IImageProcessingContext d, IEnumerable<PointF> shell, List<IEnumerable<PointF>> holes, IBrush brush, DrawingOptions drawingOptions)
        {
            var pb = new PathBuilder();
            pb.AddLines(shell).CloseFigure();
            foreach (var hole in holes)
            {
                pb.AddLines(hole).CloseFigure();
            }
            d.Fill(drawingOptions, brush, pb.Build());
        }
    }
}
