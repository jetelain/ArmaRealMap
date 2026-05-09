using GameRealisticMap;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test
{
    public class TerrainAreaExtensionsTest
    {
        private readonly TerrainAreaUTM _area;

        public TerrainAreaExtensionsTest()
        {
            // Small 1000 m terrain, 10 m cells
            _area = TerrainAreaUTM.CreateFromSouthWest("48.666667 N, 6.166667 E", 10f, 100);
        }

        [Fact]
        public void IsInside_PointAtOrigin_ReturnsTrue()
        {
            Assert.True(_area.IsInside(new TerrainPoint(0f, 0f)));
        }

        [Fact]
        public void IsInside_PointInsideBounds_ReturnsTrue()
        {
            Assert.True(_area.IsInside(new TerrainPoint(500f, 500f)));
        }

        [Fact]
        public void IsInside_PointAtMaxEdge_ReturnsFalse()
        {
            // SizeInMeters is exclusive
            Assert.False(_area.IsInside(new TerrainPoint(_area.SizeInMeters, _area.SizeInMeters)));
        }

        [Fact]
        public void IsInside_NegativeX_ReturnsFalse()
        {
            Assert.False(_area.IsInside(new TerrainPoint(-1f, 500f)));
        }

        [Fact]
        public void IsInside_NegativeY_ReturnsFalse()
        {
            Assert.False(_area.IsInside(new TerrainPoint(500f, -1f)));
        }

        [Fact]
        public void IsInside_BeyondWidth_ReturnsFalse()
        {
            Assert.False(_area.IsInside(new TerrainPoint(_area.SizeInMeters + 1f, 500f)));
        }
    }
}
