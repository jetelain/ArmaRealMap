using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.ElevationModel;
using GameRealisticMap.Geometries;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Xunit;

namespace ArmaRealMap.Test.ElevationModel
{
    public class ElevationGridTest
    {
        [Fact]
        public void ElevationGrid_ElevationAt()
        {
            var area = new MapInfos()
            {
                P1 = new TerrainPoint(10, 10),
                P2 = new TerrainPoint(30, 30),
                CellSize = 10,
                Size = 2
            };

            var grid = new ElevationGrid(area);
            grid.elevationGrid[0, 0] = 100f;
            grid.elevationGrid[1, 0] = 120f;
            grid.elevationGrid[0, 1] = 130f;
            grid.elevationGrid[1, 1] = 140f;

            Assert.Equal(100f, grid.ElevationAt(new TerrainPoint(10, 10)));
            Assert.Equal(120f, grid.ElevationAt(new TerrainPoint(30, 10)));
            Assert.Equal(130f, grid.ElevationAt(new TerrainPoint(10, 30)));
            Assert.Equal(140f, grid.ElevationAt(new TerrainPoint(30, 30)));

            Assert.Equal(110f, grid.ElevationAt(new TerrainPoint(15, 5)));
            Assert.Equal(135f, grid.ElevationAt(new TerrainPoint(15, 25)));
            Assert.Equal(115f, grid.ElevationAt(new TerrainPoint(5, 15)));
            Assert.Equal(130f, grid.ElevationAt(new TerrainPoint(25, 15)));

            Assert.Equal(107.5f, grid.ElevationAt(new TerrainPoint(10, 12.5f)));

            Assert.Equal(125, grid.ElevationAt(new TerrainPoint(15, 15)));
            Assert.Equal(140, grid.ElevationAt(new TerrainPoint(20, 20)));
        }

        [Fact]
        public void ElevationGrid_Mutate()
        {
            var area = new MapInfos()
            {
                P1 = new TerrainPoint(0, 0),
                P2 = new TerrainPoint(50, 50),
                CellSize = 10,
                Size = 5
            };
            var grid = new ElevationGrid(area);
            Assert.Equal(@"ncols         5
nrows         5
xllcorner     200000
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
", grid.DumpAsc());

            var mutate = grid.PrepareToMutate(new TerrainPoint(5, 5), new TerrainPoint(35, 35), -5, +10);
            mutate.Image.Mutate(m => {
                DrawHelper.DrawPolygon(m, TerrainPolygon.FromRectangle(new TerrainPoint(10, 10), new TerrainPoint(30, 30)), new SolidBrush(mutate.ElevationToColor(5f)), mutate.ToPixels);
                });
            mutate.Apply();

            Assert.Equal(@"ncols         5
nrows         5
xllcorner     200000
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
0.00 5.00 5.00 0.00 0.00 
0.00 5.00 5.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
", grid.DumpAsc());

            mutate = grid.PrepareToMutate(new TerrainPoint(15, 15), new TerrainPoint(45, 45), -5, +10);
            mutate.Image.Mutate(m => {
                DrawHelper.DrawPolygon(m, TerrainPolygon.FromRectangle(new TerrainPoint(20, 20), new TerrainPoint(40, 40)), new SolidBrush(mutate.ElevationToColor(10f).WithAlpha(0.5f)), mutate.ToPixels);
            });
            mutate.Apply();

            Assert.Equal(@"ncols         5
nrows         5
xllcorner     200000
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00 
0.00 0.00 5.00 5.00 0.00 
0.00 5.00 7.50 5.00 0.00 
0.00 5.00 5.00 0.00 0.00 
0.00 0.00 0.00 0.00 0.00 
", grid.DumpAsc());

            mutate = grid.PrepareToMutate(new TerrainPoint(5, 5), new TerrainPoint(45, 45), -5, +10);
            mutate.Image.Mutate(m => {
                DrawHelper.DrawPolygon(m, TerrainPolygon.FromRectangle(new TerrainPoint(10, 10), new TerrainPoint(40, 40)), new SolidBrush(mutate.ElevationToColor(7.5f).WithAlpha(0.5f)), mutate.ToPixels);
            });
            mutate.Apply();

            Assert.Equal(@"ncols         5
nrows         5
xllcorner     200000
yllcorner     0
cellsize      10
NODATA_value  -9999
0.00 0.00 0.00 0.00 0.00 
0.00 3.75 6.25 6.25 0.00 
0.00 6.25 7.50 6.25 0.00 
0.00 6.25 6.25 3.75 0.00 
0.00 0.00 0.00 0.00 0.00 
", grid.DumpAsc());

        }

    }
}
