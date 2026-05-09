using GameRealisticMap;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test
{
    public class TerrainAreaUTMTest
    {
        [Fact]
        public void CreateFromSouthWest_SetsExpectedSize()
        {
            var area = TerrainAreaUTM.CreateFromSouthWest("48.666667 N, 6.166667 E", 10f, 100);

            Assert.Equal(10f, area.GridCellSize);
            Assert.Equal(100, area.GridSize);
            Assert.Equal(1000f, area.SizeInMeters);
        }

        [Fact]
        public void CreateFromCenter_SetsExpectedSize()
        {
            var area = TerrainAreaUTM.CreateFromCenter("48.666667 N, 6.166667 E", 10f, 100);

            Assert.Equal(10f, area.GridCellSize);
            Assert.Equal(100, area.GridSize);
            Assert.Equal(1000f, area.SizeInMeters);
        }

        [Fact]
        public void LatLngToTerrainPoint_SouthWestCorner_ReturnsOrigin()
        {
            var southWest = "48.666667 N, 6.166667 E";
            var area = TerrainAreaUTM.CreateFromSouthWest(southWest, 10f, 100);

            // The south-west corner itself should map very close to (0, 0)
            var point = area.LatLngToTerrainPoint(new GeoAPI.Geometries.Coordinate(6.166667, 48.666667));

            Assert.InRange(point.X, -5f, 5f);
            Assert.InRange(point.Y, -5f, 5f);
        }

        [Fact]
        public void TerrainPointToLatLng_RoundTrip()
        {
            var area = TerrainAreaUTM.CreateFromSouthWest("48.666667 N, 6.166667 E", 10f, 512);
            var original = new TerrainPoint(500f, 300f);

            var latLng = area.TerrainPointToLatLng(original);
            var roundTripped = area.LatLngToTerrainPoint(latLng);

            Assert.Equal(original.X, roundTripped.X, 1);
            Assert.Equal(original.Y, roundTripped.Y, 1);
        }

        [Fact]
        public void TerrainBounds_CoversFullArea()
        {
            var area = TerrainAreaUTM.CreateFromSouthWest("48.666667 N, 6.166667 E", 10f, 100);

            // TerrainBounds is a rectangle from (0,0) to (SizeInMeters, SizeInMeters)
            var shell = area.TerrainBounds.Shell;
            Assert.Contains(shell, p => p.X == 0f && p.Y == 0f);
            Assert.Contains(shell, p => p.X == area.SizeInMeters && p.Y == area.SizeInMeters);
        }
    }
}
