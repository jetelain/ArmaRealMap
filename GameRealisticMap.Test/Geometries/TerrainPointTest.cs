using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPointTest
    {
        [Fact]
        public void TerrainPoint_ToIntPointPrecision()
        {
            var p = new TerrainPoint(1.23456789f, 9.87654321f);
            Assert.Equal(1.23456789f, p.X);
            Assert.Equal(9.87654321f, p.Y);

            p = p.ToIntPointPrecision();
            Assert.Equal(1.234f, p.X);
            Assert.Equal(9.876f, p.Y);
        }
    }
}
