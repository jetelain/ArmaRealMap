using System;
using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Geometries;
using ArmaRealMap.Osm;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    public class DebugImage
    {
        private readonly TerrainPoint min;
        private readonly TerrainPoint max;
        private readonly float scale;
        private readonly IImageProcessingContext p;

        private DebugImage(TerrainPoint min, TerrainPoint max, float scale, IImageProcessingContext p)
        {
            this.min = min;
            this.max = max;
            this.scale = scale;
            this.p = p;
        }

        private PointF Project(TerrainPoint p)
        {
            return new PointF((p.X - min.X) * scale, (max.Y - p.Y) * scale);
        }

        private IEnumerable<PointF> Project(IEnumerable<TerrainPoint> p)
        {
            return p.Select(Project);
        }

        internal void Fill(OsmShape shape, IBrush brush)
        {
            foreach(var poly in shape.TerrainPolygons)
            {
                Fill(poly, brush);
            }
        }

        public void Fill(TerrainPolygon polygon, IBrush brush)
        {
            try
            {
                DrawHelper.FillPolygonWithHoles(p, polygon.Shell.Select(Project), polygon.Holes.Select(Project).ToList(), brush, new DrawingOptions());
            }
            catch
            {

            }
        }

        public void Draw(TerrainPolygon polygon, IBrush brush, float width = 1f)
        {
            try
            {
                DrawHelper.DrawPolygonEdgesWithHoles(p, polygon.Shell.Select(Project), polygon.Holes.Select(Project).ToList(), brush, new DrawingOptions(), width);
            }
            catch
            {

            }
        }

        public static void Image(TerrainPoint min, TerrainPoint max, float scale, string filename, Action<DebugImage> action)
        {
            var size = max.Vector - min.Vector;
            using (var img = new Image<Rgba32>((int)MathF.Round(size.X * scale), (int)MathF.Round(size.Y * scale), Color.YellowGreen))
            {
                img.Mutate(p => action(new DebugImage(min, max, scale, p)));
                img.Save(filename);
            }
        }
    }
}
