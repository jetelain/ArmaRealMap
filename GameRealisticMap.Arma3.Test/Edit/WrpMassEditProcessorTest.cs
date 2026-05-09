using System.Numerics;
using BIS.Core.Math;
using BIS.WRP;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Test.Edit
{
    public class WrpMassEditProcessorTest
    {
        private static ModelInfoLibrary CreateEmptyLibrary()
        {
            return new ModelInfoLibrary(new GameEngine.GameFileSystemMock());
        }

        private static EditableWrp CreateSampleWorld(params (string model, float x, float y, float z)[] objects)
        {
            var world = new EditableWrp();
            world.LandRangeX = world.LandRangeY = 4;
            world.TerrainRangeX = world.TerrainRangeY = 4;
            world.CellSize = 2;
            world.Elevation = new float[16];
            world.Objects = objects.Select((o, i) => new EditableWrpObject()
            {
                Model = o.model,
                Transform = new Matrix4P(Matrix4x4.CreateTranslation(o.x, o.y, o.z)),
                ObjectID = i + 1
            }).Concat(new[] { EditableWrpObject.Dummy }).ToList();
            return world;
        }

        [Fact]
        public void Process_Reduce_RemovesAllMatchingObjects()
        {
            var world = CreateSampleWorld(
                ("path\\to\\model_a.p3d", 10, 0, 10),
                ("path\\to\\model_a.p3d", 20, 0, 20),
                ("path\\to\\model_b.p3d", 30, 0, 30)
            );

            var batch = new WrpMassEditBatch();
            batch.Reduce.Add(new WrpMassReduce("path\\to\\model_a.p3d", 1.0));

            var processor = new WrpMassEditProcessor(new NoProgress(), CreateEmptyLibrary());
            var changes = processor.Process(world, batch);

            Assert.Equal(2, changes);
            var nonDummy = world.Objects.Where(o => !string.IsNullOrEmpty(o.Model)).ToList();
            Assert.Single(nonDummy);
            Assert.Equal("path\\to\\model_b.p3d", nonDummy[0].Model);
        }

        [Fact]
        public void Process_Reduce_NoMatchingObjects_ReturnsZero()
        {
            var world = CreateSampleWorld(
                ("path\\to\\model_b.p3d", 10, 0, 10)
            );

            var batch = new WrpMassEditBatch();
            batch.Reduce.Add(new WrpMassReduce("path\\to\\model_a.p3d", 1.0));

            var processor = new WrpMassEditProcessor(new NoProgress(), CreateEmptyLibrary());
            var changes = processor.Process(world, batch);

            Assert.Equal(0, changes);
            Assert.Equal(2, world.Objects.Count); // 1 real + 1 dummy
        }

        [Fact]
        public void Process_Replace_ChangesModelAndPosition()
        {
            var world = CreateSampleWorld(
                ("path\\to\\model_a.p3d", 10, 5, 20),
                ("path\\to\\model_b.p3d", 30, 0, 30)
            );

            var batch = new WrpMassEditBatch();
            batch.Replace.Add(new WrpMassReplace("path\\to\\model_a.p3d", "path\\to\\model_c.p3d")
            {
                YShift = 2.0
            });

            var processor = new WrpMassEditProcessor(new NoProgress(), CreateEmptyLibrary());
            var changes = processor.Process(world, batch);

            Assert.Equal(1, changes);
            var nonDummy = world.Objects.Where(o => !string.IsNullOrEmpty(o.Model)).ToList();
            Assert.Equal(2, nonDummy.Count);
            var replaced = nonDummy.First(o => o.Model == "path\\to\\model_c.p3d");
            Assert.Equal(7f, replaced.Transform.Matrix.M42, 4f); // y = 5 + 2
        }

        [Fact]
        public void Process_Replace_NoMatch_ReturnsZero()
        {
            var world = CreateSampleWorld(
                ("path\\to\\model_b.p3d", 10, 5, 20)
            );

            var batch = new WrpMassEditBatch();
            batch.Replace.Add(new WrpMassReplace("path\\to\\model_a.p3d", "path\\to\\model_c.p3d")
            {
                YShift = 0.0
            });

            var processor = new WrpMassEditProcessor(new NoProgress(), CreateEmptyLibrary());
            var changes = processor.Process(world, batch);

            Assert.Equal(0, changes);
        }

        [Fact]
        public void Process_ObjectsGetResequencedIds()
        {
            var world = CreateSampleWorld(
                ("path\\to\\model_a.p3d", 10, 0, 10),
                ("path\\to\\model_a.p3d", 20, 0, 20),
                ("path\\to\\model_b.p3d", 30, 0, 30)
            );

            var batch = new WrpMassEditBatch();
            batch.Reduce.Add(new WrpMassReduce("path\\to\\model_a.p3d", 1.0));

            var processor = new WrpMassEditProcessor(new NoProgress(), CreateEmptyLibrary());
            processor.Process(world, batch);

            for (int i = 0; i < world.Objects.Count; i++)
            {
                Assert.Equal(i + 1, world.Objects[i].ObjectID);
            }
        }

        [Fact]
        public void Process_EmptyBatch_ReturnsZeroChanges()
        {
            var world = CreateSampleWorld(
                ("path\\to\\model_a.p3d", 10, 0, 10)
            );

            var batch = new WrpMassEditBatch();

            var processor = new WrpMassEditProcessor(new NoProgress(), CreateEmptyLibrary());
            var changes = processor.Process(world, batch);

            Assert.Equal(0, changes);
        }
    }
}
