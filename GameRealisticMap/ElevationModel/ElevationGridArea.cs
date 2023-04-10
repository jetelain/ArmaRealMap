using System.Numerics;
using GameRealisticMap.Geometries;
using GeoAPI.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationGridArea
    {
        private ElevationGrid elevationGrid;
        private int startX;
        private int startY;
        private Image<Rgba64> image;
        private float minElevation;
        private float elevationDelta;

        public ElevationGridArea(ElevationGrid elevationGrid, int startX, int startY, int width, int height, float minElevation, float elevationDelta)
        {
            this.elevationGrid = elevationGrid;
            this.startX = startX;
            this.startY = startY;
            this.image = new Image<Rgba64>(width, height, Color.Transparent);
            this.minElevation = minElevation;
            this.elevationDelta = elevationDelta;
        }

        public Color ElevationToColor(float elevation)
        {
            return new Color(new Vector4(0,0, (elevation - minElevation) / elevationDelta, 1f));
        }

        public PointF ToPixel(TerrainPoint point)
        {
            var grid = elevationGrid.ToGrid(point);
            return new PointF(grid.X - startX, grid.Y - startY);
        }

        public IEnumerable<PointF> ToPixels(IEnumerable<TerrainPoint> points)
        {
            return points.Select(ToPixel);
        }

        public PointF ToPixel(Coordinate point)
        {
            var grid = elevationGrid.ToGrid(new TerrainPoint((float)point.X, (float)point.Y));
            return new PointF(grid.X - startX, grid.Y - startY);
        }

        public IEnumerable<PointF> ToPixels(IEnumerable<Coordinate> points)
        {
            return points.Select(ToPixel);
        }

        public Image<Rgba64> Image => image;

        public void Apply()
        {
            Apply(ApplyAsIs);
        }

        public void ApplyAsMinimal()
        {
            Apply(ApplyAsMinimal);
        }

        private void Apply(Action<int,int,ushort,float> apply)
        {
            for (int x = 0; x < image.Width; ++x)
            {
                for (int y = 0; y < image.Height; ++y)
                {
                    if (x + startX >= 0 && y + startY >= 0 && x + startX < elevationGrid.Size && y + startY < elevationGrid.Size)
                    {
                        var pixel = image[x, y];
                        var alpha = pixel.A;
                        if (alpha != ushort.MinValue)
                        {
                            var pixelElevation = minElevation + (elevationDelta * pixel.B / (float)ushort.MaxValue);
                            apply(x, y, alpha, pixelElevation);
                        }
                    }
                }
            }
        }

        private void ApplyAsIs(int x, int y, ushort alpha, float pixelElevation)
        {
            if (alpha == ushort.MaxValue)
            {
                elevationGrid[x + startX, y + startY] = pixelElevation;
            }
            else
            {
                var existingElevation = elevationGrid[x + startX, y + startY];
                elevationGrid[x + startX, y + startY] = existingElevation + ((pixelElevation - existingElevation) * alpha / (float)ushort.MaxValue);
            }
        }


        private void ApplyAsMinimal(int x, int y, ushort alpha, float pixelElevation)
        {
            var existingElevation = elevationGrid[x + startX, y + startY];
            if (alpha != ushort.MaxValue)
            {
                pixelElevation = existingElevation + ((pixelElevation - existingElevation) * alpha / (float)ushort.MaxValue);
            }
            elevationGrid[x + startX, y + startY] = Math.Max(pixelElevation, existingElevation);
        }
    }
}