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
        public void PlaceOnPathRightAngle_Corner()
        {
            var definition = new TestSegmentsDefinition(
                new TestCornerOrEndSegmentDefinition("Left"),
                new TestCornerOrEndSegmentDefinition("Right"),
                null, 
                new TestStraightSegmentDefinition("Size4", 4), new TestStraightSegmentDefinition("Size8", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
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

        [Fact]
        public void PlaceOnPathRightAngle_Corner_Square()
        {
            var definition = new TestSegmentsDefinition(
                new TestCornerOrEndSegmentDefinition("Left"),
                new TestCornerOrEndSegmentDefinition("Right"),
                null,
                new TestStraightSegmentDefinition("Size5", 5));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, TerrainPath.FromRectangle(new TerrainPoint(8, 8), new TerrainPoint(18, 18)).Points);
            
            Assert.Equal(12, list.Count);

            var item = list[0];
            Assert.Equal(new TerrainPoint(10.5f, 8), item.Center);
            Assert.Equal("Size5", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(15.5f, 8), item.Center);
            Assert.Equal("Size5", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(18, 8), item.Center);
            Assert.Equal("Right", item.Model);
            Assert.Equal(0, item.Angle);

            // ----

            item = list[3];
            Assert.Equal(new TerrainPoint(18, 10.5f), item.Center);
            Assert.Equal("Size5", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(18, 15.5f), item.Center);
            Assert.Equal("Size5", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[5];
            Assert.Equal(new TerrainPoint(18, 18), item.Center);
            Assert.Equal("Right", item.Model);
            Assert.Equal(90, item.Angle);

            // ----

            item = list[11];
            Assert.Equal(new TerrainPoint(8, 8), item.Center);
            Assert.Equal("Right", item.Model);
            Assert.Equal(-90, item.Angle);
        }

        [Fact]
        public void PlaceOnPathRightAngle_Ends()
        {
            var definition = new TestSegmentsDefinition(
                null,
                null,
                new TestCornerOrEndSegmentDefinition("End"),
                new TestStraightSegmentDefinition("Size4", 4), new TestStraightSegmentDefinition("Size8", 8));

            var list = new List<PlacedModel<string>>();
            FollowPathWithObjects.PlaceOnPathRightAngle(new[] { definition }, list, new[] {
                new TerrainPoint(0,0),
                new TerrainPoint(0,12),
                new TerrainPoint(10,12),
                new TerrainPoint(10,20),
            });
            Assert.Equal(7, list.Count);

            var item = list[0];
            Assert.Equal(new TerrainPoint(0, 0), item.Center);
            Assert.Equal("End", item.Model);
            Assert.Equal(90, item.Angle);

            item = list[1];
            Assert.Equal(new TerrainPoint(0, 4), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[2];
            Assert.Equal(new TerrainPoint(0, 10), item.Center);
            Assert.Equal("Size4", item.Model);
            Assert.Equal(270, item.Angle);

            item = list[3];
            Assert.Equal(new TerrainPoint(4, 12), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[4];
            Assert.Equal(new TerrainPoint(8, 12), item.Center);
            Assert.Equal("Size4", item.Model);
            Assert.Equal(180, item.Angle);

            item = list[5];
            Assert.Equal(new TerrainPoint(10, 16), item.Center);
            Assert.Equal("Size8", item.Model);
            Assert.Equal(270, item.Angle); 
            
            item = list[6];
            Assert.Equal(new TerrainPoint(10, 20), item.Center);
            Assert.Equal("End", item.Model);
            Assert.Equal(90, item.Angle);
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
