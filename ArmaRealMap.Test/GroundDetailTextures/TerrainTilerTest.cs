using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.TerrainData.GroundDetailTextures;
using Xunit;

namespace ArmaRealMap.Test.GroundDetailTextures
{
    public class TerrainTilerTest
    {
        [Fact]
        public void TerrainTiler_UVB()
        {
            var point = "30P XB 62600 87000";

            var gossi = new TerrainTiler(new MapConfig() { CellSize = 10, GridSize = 8192, Resolution = 2, BottomLeft = point });

            Assert.Equal(0.03125, gossi.Segments[0, 0].UB);
            Assert.Equal(40.03125, gossi.Segments[0, 0].VB);

            Assert.Equal(0.03125, gossi.Segments[0, 42].UB);
            Assert.Equal(0.65625, gossi.Segments[0, 42].VB);

            Assert.Equal(-39.34375, gossi.Segments[42, 0].UB);
            Assert.Equal(40.03125, gossi.Segments[42, 0].VB);

            Assert.Equal(-39.34375, gossi.Segments[42, 42].UB);
            Assert.Equal(0.65625, gossi.Segments[42, 42].VB); 
            
            var taunus = new TerrainTiler(new MapConfig() { CellSize = 5, GridSize = 4096, Resolution = 2, BottomLeft = point, TileSize = 512 });

            Assert.Equal(0.03125, taunus.Segments[0, 0].UB);
            Assert.Equal(20.03125, taunus.Segments[0, 0].VB);

            Assert.Equal(0.03125, taunus.Segments[0, 21].UB);
            Assert.Equal(0.34375, taunus.Segments[0, 21].VB);

            Assert.Equal(-19.65625, taunus.Segments[21, 0].UB);
            Assert.Equal(20.03125, taunus.Segments[21, 0].VB);

            Assert.Equal(-19.65625, taunus.Segments[21, 21].UB);
            Assert.Equal(0.34375, taunus.Segments[21, 21].VB);


            var belfort = new TerrainTiler(new MapConfig() { CellSize = 5, GridSize = 4096, Resolution = 1, BottomLeft = point, TileSize = 1024 });

            Assert.Equal(0.03125, belfort.Segments[0, 0].UB);
            Assert.Equal(20.03125, belfort.Segments[0, 0].VB);

            Assert.Equal(0.03125, belfort.Segments[0, 21].UB);
            Assert.Equal(0.34375, belfort.Segments[0, 21].VB);

            Assert.Equal(-19.65625, belfort.Segments[21, 0].UB);
            Assert.Equal(20.03125, belfort.Segments[21, 0].VB);

            Assert.Equal(-19.65625, belfort.Segments[21, 21].UB);
            Assert.Equal(0.34375, belfort.Segments[21, 21].VB);
        }


        [Fact]
        public void TerrainTiler_UA()
        {
            var point = "30P XB 62600 87000";

            var gossi = new TerrainTiler(new MapConfig() { CellSize = 10, GridSize = 8192, Resolution = 2, BottomLeft = point });

            Assert.Equal(0.000488281250, gossi.Segments[0, 0].UA);

            var taunus = new TerrainTiler(new MapConfig() { CellSize = 5, GridSize = 4096, Resolution = 2, BottomLeft = point, TileSize = 512 });

            Assert.Equal(0.0009765625, taunus.Segments[0, 0].UA);

            var belfort = new TerrainTiler(new MapConfig() { CellSize = 5, GridSize = 4096, Resolution = 1, BottomLeft = point, TileSize = 1024 });

            Assert.Equal(0.0009765625, belfort.Segments[0, 0].UA);
        }
    }
}
