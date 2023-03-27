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
            Assert.Equal(0, box.Angle);
            Assert.Equal(18, box.Width);
            Assert.Equal(10, box.Height);
            Assert.Equal(9, box.Center.X);
            Assert.Equal(5, box.Center.Y);
        }
    }
}
