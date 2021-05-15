using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
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

            Assert.Equal(110f, grid.ElevationAt(new TerrainPoint(20, 10)));
            Assert.Equal(135f, grid.ElevationAt(new TerrainPoint(20, 30)));
            Assert.Equal(115f, grid.ElevationAt(new TerrainPoint(10, 20)));
            Assert.Equal(130f, grid.ElevationAt(new TerrainPoint(30, 20)));

            Assert.Equal(122.5f, grid.ElevationAt(new TerrainPoint(20, 20)));
        }
    }
}
