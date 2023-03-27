using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameRealisticMap.Geometries;
using NetTopologySuite.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.ElevationModel
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
            elevationGrid.Apply(startX, startY, image, minElevation, elevationDelta);
        }
    }
}