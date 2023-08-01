using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Algorithms.Following
{
    public class FollowPathWithObjectsTest
    {
        [Fact]
        public void PlaceOnPathRightAngle()
        {
            var definition = new TestSegmentsDefinition(new TestStraightSegmentDefinition("Size4", 4), new TestStraightSegmentDefinition("Size8", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,12),
                new TerrainPoint(10,12),
                new TerrainPoint(10,20),
            });
            Assert.Equal(5, list.Count);

            var item = list[0];
            Assert.Equal(new TerrainPoint(0,4), item.Center);
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
        public void PlaceOnPathRightAngle_Random_UseLongest()
        {
            var definition = new TestSegmentsDefinition(
                new TestStraightSegmentDefinition("Size4A", 4),
                new TestStraightSegmentDefinition("Size4B", 4),
                new TestStraightSegmentDefinition("Size4C", 4),
                new TestStraightSegmentDefinition("Size8A", 8),
                new TestStraightSegmentDefinition("Size8B", 8),
                new TestStraightSegmentDefinition("Size8C", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,12),
                new TerrainPoint(10,12),
                new TerrainPoint(10,20),
            });
            Assert.Equal(5, list.Count);

            // We use a stable pseudo random, that is based on corrdinates of first point
            // For a same path, we always have the same sequence

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8C", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 10), item.Center);
            Assert.Equal("Size4C", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(4, 12), item.Center);
            Assert.Equal("Size8B", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(8, 12), item.Center);
            Assert.Equal("Size4B", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(10, 16), item.Center);
            Assert.Equal("Size8C", item.Model);
            Assert.Equal(270, item.Angle);
        }

        [Fact]
        public void PlaceOnPathRightAngle_Random_UseLongest_Straight()
        {
            var definition = new TestSegmentsDefinition(
                new TestStraightSegmentDefinition("Size4A", 4),
                new TestStraightSegmentDefinition("Size4B", 4),
                new TestStraightSegmentDefinition("Size4C", 4),
                new TestStraightSegmentDefinition("Size8A", 8),
                new TestStraightSegmentDefinition("Size8B", 8),
                new TestStraightSegmentDefinition("Size8C", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,40)
            });
            Assert.Equal(5, list.Count);

            // We use a stable pseudo random, that is based on corrdinates of first point
            // For a same path, we always have the same sequence

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8C", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 12), item.Center);
            Assert.Equal("Size8C", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(0, 20), item.Center);
            Assert.Equal("Size8C", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(0, 28), item.Center);
            Assert.Equal("Size8B", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(0, 36), item.Center);
            Assert.Equal("Size8A", item.Model);
            Assert.Equal(270, item.Angle);
        }

        [Fact]
        public void PlaceOnPathRightAngle_Random_UseAnySize()
        {
            var definition = new TestSegmentsDefinition(
                new TestStraightSegmentDefinition("Size4A", 4),
                new TestStraightSegmentDefinition("Size4B", 4),
                new TestStraightSegmentDefinition("Size4C", 4),
                new TestStraightSegmentDefinition("Size8A", 8),
                new TestStraightSegmentDefinition("Size8B", 8),
                new TestStraightSegmentDefinition("Size8C", 8))
            { UseAnySize = true };

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,12),
                new TerrainPoint(10,12),
                new TerrainPoint(10,20),
            });
            Assert.Equal(5, list.Count);

            // We use a stable pseudo random, that is based on corrdinates of first point
            // For a same path, we always have the same sequence

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8B", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 10), item.Center);
            Assert.Equal("Size4C", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(4, 12), item.Center);
            Assert.Equal("Size8A", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(8, 12), item.Center);
            Assert.Equal("Size4B", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(10, 16), item.Center);
            Assert.Equal("Size8C", item.Model);
            Assert.Equal(270, item.Angle);
        }

        [Fact]
        public void PlaceOnPathRightAngle_Random_UseAnySize_Straight()
        {
            var definition = new TestSegmentsDefinition(
                new TestStraightSegmentDefinition("Size4A", 4),
                new TestStraightSegmentDefinition("Size4B", 4),
                new TestStraightSegmentDefinition("Size4C", 4),
                new TestStraightSegmentDefinition("Size8A", 8),
                new TestStraightSegmentDefinition("Size8B", 8),
                new TestStraightSegmentDefinition("Size8C", 8))
            { UseAnySize = true };

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,40)
            });
            Assert.Equal(6, list.Count);

            // We use a stable pseudo random, that is based on corrdinates of first point
            // For a same path, we always have the same sequence

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8B", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 12), item.Center);
            Assert.Equal("Size8B", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(0, 20), item.Center);
            Assert.Equal("Size8B", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(0, 28), item.Center);
            Assert.Equal("Size8A", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(0, 34), item.Center);
            Assert.Equal("Size4B", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[5];
            Assert.Equal(new TerrainPoint(0, 38), item.Center);
            Assert.Equal("Size4C", item.Model);
            Assert.Equal(270, item.Angle);
        }
    }
}
