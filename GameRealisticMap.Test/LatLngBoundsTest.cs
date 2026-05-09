using GeoAPI.Geometries;
using GameRealisticMap;

namespace GameRealisticMap.Test
{
    public class LatLngBoundsTest
    {
        [Fact]
        public void Constructor_WithCoordinates_SetsProperties()
        {
            var bounds = new LatLngBounds(1.0, 50.0, 2.0, 49.0);

            Assert.Equal(1.0, bounds.Left);
            Assert.Equal(2.0, bounds.Right);
            Assert.Equal(50.0, bounds.Top);
            Assert.Equal(49.0, bounds.Bottom);
        }

        [Fact]
        public void Constructor_WithPoints_ComputesMinMax()
        {
            var points = new List<Coordinate>
            {
                new Coordinate(1.5, 49.5),
                new Coordinate(2.5, 50.5),
                new Coordinate(1.0, 50.0),
            };

            var bounds = new LatLngBounds(points);

            Assert.Equal(1.0, bounds.Left);
            Assert.Equal(2.5, bounds.Right);
            Assert.Equal(50.5, bounds.Top);
            Assert.Equal(49.5, bounds.Bottom);
        }

        [Fact]
        public void Name_ReturnsInvariantString()
        {
            var bounds = new LatLngBounds(1.0, 50.0, 2.0, 49.0);

            Assert.Equal("1_49_2_50", bounds.Name);
        }

        [Fact]
        public void ToString_ReturnsSameAsName()
        {
            var bounds = new LatLngBounds(1.5, 50.5, 2.5, 49.5);

            Assert.Equal(bounds.Name, bounds.ToString());
        }
    }
}
