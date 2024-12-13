using BIS.WRP;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.ElevationModel;
using Pmad.Cartography.DataCells;

namespace GameRealisticMap.Arma3.Test.GameEngine
{
    public class EditableWrpHelperTest
    {
        [Fact]
        public void ToElevationGrid()
        {
            var world = new EditableWrp();
            world.LandRangeX = world.LandRangeY = 2;
            world.TerrainRangeX = world.TerrainRangeY = 4;
            world.CellSize = 2;
            world.Elevation = new float[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 0, 2, 3,
                4, 5, 6, 7 };

            var grid = world.ToElevationGrid();
            Assert.Equal(1, grid.CellSize.X);
            Assert.Equal(4, grid.Size);
            Assert.Equal(1, grid[0, 0]);
            Assert.Equal(7, grid[3, 3]);
            Assert.Equal(new float[,] {
                {1, 2, 3, 4},
                {5, 6, 7, 8},
                {9, 0, 2, 3},
                {4, 5, 6, 7} }, grid.ToDataCell().Data);
        }

        [Fact]
        public void FillFromElevationGrid()
        {
            var grid = new ElevationGrid(new DemDataCellPixelIsPoint<float>(new Pmad.Cartography.Coordinates(0, 0), new Pmad.Cartography.Coordinates(3, 3), new float[,] {
                {1, 2, 3, 4},
                {5, 6, 7, 8},
                {9, 0, 2, 3},
                {4, 5, 6, 7} }));

            var world = new EditableWrp();
            world.LandRangeX = world.LandRangeY = 2;
            world.TerrainRangeX = world.TerrainRangeY = 4;
            world.CellSize = 2;
            world.FillFromElevationGrid(grid);
            Assert.Equal(new float[] {
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 0, 2, 3,
                4, 5, 6, 7 }, world.Elevation);
        }
    }
}
