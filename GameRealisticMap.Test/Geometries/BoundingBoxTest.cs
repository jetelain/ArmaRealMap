using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class BoundingBoxTest
    {
        [Fact]
        public void BoundingBox_ComputeInner()
        {
            var box = BoundingBox.ComputeInner(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(20,0),
                new TerrainPoint(20,8),
                new TerrainPoint(18,10),
                new TerrainPoint(0,10)
            });
            Assert.NotNull(box);
            Assert.Equal(0, box.Angle);
            Assert.Equal(18, box.Width);
            Assert.Equal(10, box.Height);
            Assert.Equal(9, box.Center.X);
            Assert.Equal(5, box.Center.Y);
        }

        [Fact]
        public void BoundingBox_Compute()
        {
            var box = BoundingBox.Compute(new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(20,0),
                new TerrainPoint(20,8),
                new TerrainPoint(18,10),
                new TerrainPoint(0,10)
            });
            Assert.NotNull(box);
            Assert.Equal(0, box.Angle);
            Assert.Equal(20, box.Width);
            Assert.Equal(10, box.Height);
            Assert.Equal(10, box.Center.X);
            Assert.Equal(5, box.Center.Y);

            box = BoundingBox.Compute(new[] {
                new TerrainPoint(0,10),
                new TerrainPoint(10,0),
                new TerrainPoint(20,10),
                new TerrainPoint(10,20)
            });
            Assert.NotNull(box);
            Assert.Equal(45, box.Angle);
            Assert.Equal(14.142, Math.Round(box.Width, 3));
            Assert.Equal(14.142, Math.Round(box.Height, 3));
            Assert.Equal(10, box.Center.X);
            Assert.Equal(10, box.Center.Y);
        }
    }
}
