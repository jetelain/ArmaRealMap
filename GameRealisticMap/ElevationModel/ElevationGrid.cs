using System.Numerics;
using GameRealisticMap.Geometries;
using MapToolkit;
using MapToolkit.DataCells;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.ElevationModel
{
    public class ElevationGrid
    {
        private readonly int size;
        private readonly float[,] elevationGrid;
        private readonly Vector2 cellSize;

        public ElevationGrid(int size, float cellSize)
        {
            this.size = size;
            this.elevationGrid = new float[size, size];
            this.cellSize = new Vector2(cellSize);
        }

        public ElevationGrid(ElevationGrid other)
        {
            size = other.size;
            elevationGrid = (float[,])other.elevationGrid.Clone();
            cellSize = other.cellSize;
        }

        public float[,] Data => elevationGrid;

        public int Size => size;

        public float CellSize => cellSize.X;

        public float ElevationAround(TerrainPoint p)
        {
            return ElevationAround(p, cellSize.X / 2);
        }

        public float ElevationAround(TerrainPoint p, float radius)
        {
            return 
                (ElevationAt(p) + 
                ElevationAt(p + new Vector2(-radius, -radius)) +
                ElevationAt(p + new Vector2(radius, -radius)) +
                ElevationAt(p + new Vector2(-radius, radius)) +
                ElevationAt(p + new Vector2(radius, radius))) / 5f;
        }

        private float ElevationAtCell(int x, int y)
        {
            return elevationGrid[
                Math.Min(Math.Max(0, x), size - 1),
                Math.Min(Math.Max(0, y), size - 1)];
        }

        public float ElevationAtGrid(Vector2 gridPos)
        {
            var x = (int)MathF.Floor(gridPos.X);
            var y = (int)MathF.Floor(gridPos.Y);
            var xIn = gridPos.X - x;
            var yIn = gridPos.Y - y;
            var z10 = ElevationAtCell(x + 1, y);
            var z01 = ElevationAtCell(x, y + 1);
            if (xIn <= 1 - yIn)
            {
                var z00 = ElevationAtCell(x, y);
                var d1000 = z10 - z00;
                var d0100 = z01 - z00;
                return z00 + d0100 * yIn + d1000 * xIn;
            }
            var z11 = ElevationAtCell(x + 1, y + 1);
            var d1011 = z10 - z11;
            var d0111 = z01 - z11;
            return z10 + d0111 - d0111 * xIn - d1011 * yIn;
        }

        public float ElevationAt(TerrainPoint point)
        {
            return ElevationAtGrid(ToGrid(point));
        }

        public Vector2 ToGrid(TerrainPoint point)
        {
            return point.Vector / cellSize;
        }

        public TerrainPoint ToTerrain(int x, int y)
        {
            return ToTerrain(new Vector2(x, y));
        }

        public TerrainPoint ToTerrain(Vector2 grid)
        {
            return new TerrainPoint(grid * cellSize);
        }

        public ElevationGridArea PrepareToMutate(TerrainPoint min, TerrainPoint max, float minElevation, float maxElevation)
        {
            var posMin = ToGrid(min);
            var posMax = ToGrid(max);
            var x1 = (int)Math.Floor(posMin.X);
            var y1 = (int)Math.Floor(posMin.Y);
            var x2 = (int)Math.Ceiling(posMax.X);
            var y2 = (int)Math.Ceiling(posMax.Y);
            var delta = maxElevation - minElevation;
            return new ElevationGridArea(this, x1, y1, x2 - x1 + 1, y2 - y1 + 1, minElevation, delta);
        }

        internal void Apply (int startX, int startY, Image<Rgba64> data, float minElevation, float elevationDelta)
        {
            for(int x = 0; x < data.Width; ++x)
            {
                for (int y = 0; y < data.Height; ++y)
                {
                    if (x + startX >= 0 && y + startY >= 0 && x + startX < size && y + startY < size)
                    {
                        var pixel = data[x, y];
                        if (pixel.A != ushort.MinValue)
                        {
                            var pixelElevation = minElevation + (elevationDelta * pixel.B / (float)ushort.MaxValue);
                            if (pixel.A == ushort.MaxValue)
                            {
                                elevationGrid[x + startX, y + startY] = pixelElevation;
                            }
                            else
                            {
                                var existingElevation = elevationGrid[x + startX, y + startY];
                                elevationGrid[x + startX, y + startY] = existingElevation + ((pixelElevation - existingElevation) * pixel.A / (float)ushort.MaxValue);
                            }
                        }
                    }
                }
            }

        }

        public DemDataCellPixelIsPoint<float> ToDataCell()
        {
            return new DemDataCellPixelIsPoint<float>(Coordinates.Zero, new MapToolkit.Vector(cellSize), elevationGrid);
        }
    }
}
