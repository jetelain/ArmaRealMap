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

        [Fact]
        public void TerrainPath_Distance()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10));
            Assert.Equal(5, path.Distance(new(5, 5)));
            Assert.Equal(5, path.Distance(new(-5, 5)));
            Assert.Equal(5, path.Distance(new(0, 15)));
            Assert.Equal(5, path.Distance(new(0, -5)));
        }

        [Fact]
        public void TerrainPath_NearestPoint()
        {
            var path = new TerrainPath(new(0, 0), new(0, 10));
            Assert.Equal(new (0, 5), path.NearestPointBoundary(new (5, 5)));
            Assert.Equal(new (0, 5), path.NearestPointBoundary(new (-5, 5)));
            Assert.Equal(new (0, 10), path.NearestPointBoundary(new (0, 15)));
            Assert.Equal(new (0, 0), path.NearestPointBoundary(new (0, -5)));
        }
    }
}
