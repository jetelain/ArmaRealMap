using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPathTest
    {
        [Fact]
        public void TerrainPath_Length()
        {
            var path = new TerrainPath(new (0, 0), new (0, 10), new (10, 10));
            Assert.Equal(20, path.Length);

            path = new TerrainPath(new(0, 0), new(10, 10));
            Assert.Equal(14.14, Math.Round(path.Length, 2));
        }
    }
}
