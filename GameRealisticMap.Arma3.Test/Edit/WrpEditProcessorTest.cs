using System.Numerics;
using BIS.Core.Math;
using BIS.WRP;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Reporting;
using MapToolkit.DataCells;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class WrpEditProcessorTest
    {
        private static EditableWrp CreateSampleWorld()
        {
            var world = new EditableWrp();
            world.LandRangeX = world.LandRangeY = 2;
            world.TerrainRangeX = world.TerrainRangeY = 4;
            world.CellSize = 2;
            world.Elevation = new float[] {
                4, 4, 4, 4,
                4, 4, 4, 4,
                4, 4, 5, 5,
                4, 4, 5, 5 };
            world.Objects = new List<EditableWrpObject>() {
                new EditableWrpObject() { Model = "model1.p3d", Transform = new Matrix4P(Matrix4x4.CreateTranslation(0.5f, 4, 0.5f)) },
                new EditableWrpObject() { Model = "model2.p3d", Transform = new Matrix4P(Matrix4x4.CreateTranslation(2.5f, 5, 2.5f)) },
            };
            return world;
        }

        [Fact]
        public void UpdateElevationGrid()
        {
            var world = CreateSampleWorld();

            var grid = new ElevationGrid(new DemDataCellPixelIsPoint<float>(new MapToolkit.Coordinates(0, 0), new MapToolkit.Coordinates(3, 3), new float[,] {
                {5, 5, 5, 5},
                {5, 5, 5, 5},
                {5, 5, 5, 5},
                {5, 5, 5, 5} }));

            new WrpEditProcessor(new NoProgressSystem()).UpdateElevationGrid(world, grid);

            Assert.Equal(new float[] {
                5, 5, 5, 5,
                5, 5, 5, 5,
                5, 5, 5, 5,
                5, 5, 5, 5 }, world.Elevation);

            Assert.Equal(Matrix4x4.CreateTranslation(0.5f, 5, 0.5f), world.Objects[0].Transform.Matrix);
            Assert.Equal(Matrix4x4.CreateTranslation(2.5f, 5, 2.5f), world.Objects[1].Transform.Matrix);
        }

        [Fact]
        public void UpdateElevationGridAbsolute()
        {
            var world = CreateSampleWorld();

            var grid = new ElevationGrid(new DemDataCellPixelIsPoint<float>(new MapToolkit.Coordinates(0, 0), new MapToolkit.Coordinates(3, 3), new float[,] {
                {5, 5, 5, 5},
                {5, 5, 5, 5},
                {5, 5, 5, 5},
                {5, 5, 5, 5} }));

            new WrpEditProcessor(new NoProgressSystem()).UpdateElevationGridAbsolute(world, grid);

            Assert.Equal(new float[] {
                5, 5, 5, 5,
                5, 5, 5, 5,
                5, 5, 5, 5,
                5, 5, 5, 5 }, world.Elevation);

            Assert.Equal(Matrix4x4.CreateTranslation(0.5f, 4, 0.5f), world.Objects[0].Transform.Matrix);
            Assert.Equal(Matrix4x4.CreateTranslation(2.5f, 5, 2.5f), world.Objects[1].Transform.Matrix);
        }

        [Fact]
        public void Process_Elevation_ElevationAdjustObjects()
        {
            var world = CreateSampleWorld();

            var batch = new WrpEditBatch();
            batch.ElevationAdjustObjects = true;
            batch.Elevation.Add(new WrpSetElevationGrid(0, 0, 5));
            batch.Elevation.Add(new WrpSetElevationGrid(0, 1, 5));
            batch.Elevation.Add(new WrpSetElevationGrid(1, 0, 5));
            batch.Elevation.Add(new WrpSetElevationGrid(1, 1, 5));

            new WrpEditProcessor(new NoProgressSystem()).Process(world, batch);

            Assert.Equal(new float[] {
                5, 5, 4, 4,
                5, 5, 4, 4,
                4, 4, 5, 5,
                4, 4, 5, 5 }, world.Elevation);

            Assert.Equal(Matrix4x4.CreateTranslation(0.5f, 5, 0.5f), world.Objects[0].Transform.Matrix);
            Assert.Equal(Matrix4x4.CreateTranslation(2.5f, 5, 2.5f), world.Objects[1].Transform.Matrix);
        }

        [Fact]
        public void Process_Elevation()
        {
            var world = CreateSampleWorld();

            var batch = new WrpEditBatch();
            batch.Elevation.Add(new WrpSetElevationGrid(0, 0, 5));
            batch.Elevation.Add(new WrpSetElevationGrid(0, 1, 5));
            batch.Elevation.Add(new WrpSetElevationGrid(1, 0, 5));
            batch.Elevation.Add(new WrpSetElevationGrid(1, 1, 5));

            new WrpEditProcessor(new NoProgressSystem()).Process(world, batch);

            Assert.Equal(new float[] {
                5, 5, 4, 4,
                5, 5, 4, 4,
                4, 4, 5, 5,
                4, 4, 5, 5 }, world.Elevation);

            Assert.Equal(Matrix4x4.CreateTranslation(0.5f, 4, 0.5f), world.Objects[0].Transform.Matrix);
            Assert.Equal(Matrix4x4.CreateTranslation(2.5f, 5, 2.5f), world.Objects[1].Transform.Matrix);
        }
    }
}
