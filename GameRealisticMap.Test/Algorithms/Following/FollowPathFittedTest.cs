using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Algorithms.Following
{
    public class FollowPathFittedTest
    {
        [Fact]
        public void PlaceOnPathRightAngleV2()
        {
            var definition = new TestSegmentsDefinition(new TestStraightSegmentDefinition("Size4", 4), new TestStraightSegmentDefinition("Size8", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngleV2(new Random(0), definition, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,12),
                new TerrainPoint(10,12),
                new TerrainPoint(10,20),
            });
            Assert.Equal(5, list.Count);

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 10), item.Center);
            Assert.Equal("Size4", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(4, 12), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(8, 12), item.Center);
            Assert.Equal("Size4", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(10, 16), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(270, item.Angle);
        }

        [Fact]
        public void PlaceOnPathRightAngleV2_Corner()
        {
            var definition = new TestSegmentsDefinition(
                new TestCornerOrEndSegmentDefinition("Left"),
                new TestCornerOrEndSegmentDefinition("Right"),
                null,
                new TestStraightSegmentDefinition("Size4", 4), new TestStraightSegmentDefinition("Size8", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngleV2(new Random(0), definition, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,12),
                new TerrainPoint(10,12),
                new TerrainPoint(10,20),
            });
            Assert.Equal(7, list.Count);

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 10), item.Center);
            Assert.Equal("Size4", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(0, 12), item.Center);
            Assert.Equal("Left", item.Model);
            Assert.Equal(90, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(4, 12), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(8, 12), item.Center);
            Assert.Equal("Size4", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[5];
            Assert.Equal(new TerrainPoint(10, 12), item.Center);
            Assert.Equal("Right", item.Model);
            Assert.Equal(0, item.Angle);

            item = list[6];
            Assert.Equal(new TerrainPoint(10, 16), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(270, item.Angle);
        }
    }
}
