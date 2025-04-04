﻿using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using Pmad.Cartography;
using Pmad.Cartography.DataCells;
using Pmad.Cartography.DataCells.FileFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Test.ElevationModel
{
    public class ElevationGridTest
    {
        [Fact]
        public void ElevationGrid_ElevationAt()
        {
            var grid = new ElevationGrid(2, 10);
            grid[0, 0] = 100f;
            grid[1, 0] = 120f;
            grid[0, 1] = 130f;
            grid[1, 1] = 140f;

            Assert.Equal(100f, grid.ElevationAt(new TerrainPoint(0, 0)));
            Assert.Equal(120f, grid.ElevationAt(new TerrainPoint(20, 0)));
            Assert.Equal(130f, grid.ElevationAt(new TerrainPoint(0, 20)));
            Assert.Equal(140f, grid.ElevationAt(new TerrainPoint(20, 20)));

            Assert.Equal(110f, grid.ElevationAt(new TerrainPoint(5, -5)));
            Assert.Equal(135f, grid.ElevationAt(new TerrainPoint(5, 15)));
            Assert.Equal(115f, grid.ElevationAt(new TerrainPoint(-5, 5)));
            Assert.Equal(130f, grid.ElevationAt(new TerrainPoint(15, 5)));

            Assert.Equal(107.5f, grid.ElevationAt(new TerrainPoint(0, 2.5f)));

            Assert.Equal(125, grid.ElevationAt(new TerrainPoint(5, 5)));
            Assert.Equal(140, grid.ElevationAt(new TerrainPoint(10, 10)));

        }

        [Fact]
        public void ElevationGrid_ToDataCell()
        {
            var grid = new ElevationGrid(2, 10);
            grid[0, 0] = 100f;
            grid[1, 0] = 120f;
            grid[0, 1] = 130f;
            grid[1, 1] = 140f;

            var cell = grid.ToDataCell();

            Assert.Equal(100f, ElevationAt(cell, 0, 0));
            Assert.Equal(107.5f, ElevationAt(cell, 0, 2.5f));
            Assert.Equal(125, ElevationAt(cell, 5, 5));
            Assert.Equal(140, ElevationAt(cell, 10, 10));
        }

        private float ElevationAt(DemDataCellPixelIsPoint<float> cell, int x, float y)
        {
            return (float)cell.GetLocalElevation(Coordinates.FromXY(x, y), TriangleNWToSEInterpolation.Instance);
        }

        [Fact]
        public void ElevationGrid_Mutate()
        {
            var grid = new ElevationGrid(5,10);
            Assert.Equal(@"ncols         5
nrows         5
xllcorner     0
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
", DumpAsc(grid));

            var mutate = grid.PrepareToMutate(new TerrainPoint(5, 5), new TerrainPoint(35, 35), -5, +10);
            mutate.Image.Mutate(m => {
                PolygonDrawHelper.DrawPolygon(m, TerrainPolygon.FromRectangle(new TerrainPoint(10, 10), new TerrainPoint(30, 30)), new SolidBrush(mutate.ElevationToColor(5f)), mutate.ToPixels);
                });
            mutate.Apply();

            Assert.Equal(@"ncols         5
nrows         5
xllcorner     0
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
0.00 5.00 5.00 0.00 0.00
0.00 5.00 5.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
", DumpAsc(grid));

            mutate = grid.PrepareToMutate(new TerrainPoint(15, 15), new TerrainPoint(45, 45), -5, +10);
            mutate.Image.Mutate(m => {
                PolygonDrawHelper.DrawPolygon(m, TerrainPolygon.FromRectangle(new TerrainPoint(20, 20), new TerrainPoint(40, 40)), new SolidBrush(mutate.ElevationToColor(10f).WithAlpha(0.5f)), mutate.ToPixels);
            });
            mutate.Apply();

            Assert.Equal(@"ncols         5
nrows         5
xllcorner     0
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00
0.00 0.00 5.00 5.00 0.00
0.00 5.00 7.50 5.00 0.00
0.00 5.00 5.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00
", DumpAsc(grid));

            mutate = grid.PrepareToMutate(new TerrainPoint(5, 5), new TerrainPoint(45, 45), -5, +10);
            mutate.Image.Mutate(m => {
                PolygonDrawHelper.DrawPolygon(m, TerrainPolygon.FromRectangle(new TerrainPoint(10, 10), new TerrainPoint(40, 40)), new SolidBrush(mutate.ElevationToColor(7.5f).WithAlpha(0.5f)), mutate.ToPixels);
            });
            mutate.Apply();

            Assert.Equal(@"ncols         5
nrows         5
xllcorner     0
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00
0.00 3.75 6.25 6.25 0.00
0.00 6.25 7.50 6.25 0.00
0.00 6.25 6.25 3.75 0.00
0.00 0.00 0.00 0.00 0.00
", DumpAsc(grid));

        }

        private static string DumpAsc(ElevationGrid grid)
        {
            var str = new StringWriter();
            EsriAsciiHelper.SaveDataCell(str, grid.ToDataCell());
            return str.ToString();
        }

        [Fact]
        public void ElevationGrid_NormalAt()
        {
            var grid = new ElevationGrid(2, 10);
            grid[0, 0] = 100f;
            grid[1, 0] = 100f;
            grid[0, 1] = 100f;
            grid[1, 1] = 100f;

            Assert.Equal(new Vector3(0, 0, 1), grid.NormalAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(new Vector3(0, 0, 1), grid.NormalAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 100f;
            grid[1, 0] = 100f;
            grid[0, 1] = 110f;
            grid[1, 1] = 110f;

            Assert.Equal(new Vector3(0, -0.70710677f, 0.70710677f), grid.NormalAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(new Vector3(0, -0.70710677f, 0.70710677f), grid.NormalAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 110f;
            grid[1, 0] = 110f;
            grid[0, 1] = 100f;
            grid[1, 1] = 100f;

            Assert.Equal(new Vector3(0, 0.70710677f, 0.70710677f), grid.NormalAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(new Vector3(0, 0.70710677f, 0.70710677f), grid.NormalAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 100f;
            grid[1, 0] = 110f;
            grid[0, 1] = 100f;
            grid[1, 1] = 110f;

            Assert.Equal(new Vector3(-0.70710677f, 0, 0.70710677f), grid.NormalAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(new Vector3(-0.70710677f, 0, 0.70710677f), grid.NormalAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 110f;
            grid[1, 0] = 100f;
            grid[0, 1] = 110f;
            grid[1, 1] = 100f;

            Assert.Equal(new Vector3(0.70710677f, 0, 0.70710677f), grid.NormalAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(new Vector3(0.70710677f, 0, 0.70710677f), grid.NormalAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 110f;
            grid[1, 0] = 110f;
            grid[0, 1] = 110f;
            grid[1, 1] = 100f;

            Assert.Equal(new Vector3(0, 0, 1), grid.NormalAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(new Vector3(0.57735026f, 0.57735026f, 0.57735026f), grid.NormalAt(new TerrainPoint(7.5f, 7.5f)));
        }


        [Fact]
        public void ElevationGrid_SlopeAt()
        {
            var grid = new ElevationGrid(2, 10);
            grid[0, 0] = 100f;
            grid[1, 0] = 100f;
            grid[0, 1] = 100f;
            grid[1, 1] = 100f;

            Assert.Equal(0, grid.SlopeAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(0, grid.SlopeAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 100f;
            grid[1, 0] = 100f;
            grid[0, 1] = 110f;
            grid[1, 1] = 110f;

            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 110f;
            grid[1, 0] = 110f;
            grid[0, 1] = 100f;
            grid[1, 1] = 100f;

            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 100f;
            grid[1, 0] = 110f;
            grid[0, 1] = 100f;
            grid[1, 1] = 110f;

            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 110f;
            grid[1, 0] = 100f;
            grid[0, 1] = 110f;
            grid[1, 1] = 100f;

            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(45, grid.SlopeAt(new TerrainPoint(7.5f, 7.5f)));

            grid[0, 0] = 110f;
            grid[1, 0] = 110f;
            grid[0, 1] = 110f;
            grid[1, 1] = 100f;

            Assert.Equal(0, grid.SlopeAt(new TerrainPoint(2.5f, 2.5f)));
            Assert.Equal(54.735607f, grid.SlopeAt(new TerrainPoint(7.5f, 7.5f)));
        }
    }
}
