using OsmSharp.Tags;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;

namespace GameRealisticMap.Test.ManMade.Roads
{
    public class RoadTypeIdHelperTest
    {
        private static TagsCollection Tags(params (string key, string value)[] pairs)
        {
            var tags = new TagsCollection();
            foreach (var (key, value) in pairs)
                tags.Add(new Tag(key, value));
            return tags;
        }

        [Theory]
        [InlineData("motorway", RoadTypeId.TwoLanesMotorway)]
        [InlineData("trunk", RoadTypeId.TwoLanesPrimaryRoad)]
        [InlineData("primary", RoadTypeId.TwoLanesPrimaryRoad)]
        [InlineData("primary_link", RoadTypeId.TwoLanesPrimaryRoad)]
        [InlineData("trunk_link", RoadTypeId.TwoLanesPrimaryRoad)]
        [InlineData("motorway_link", RoadTypeId.TwoLanesPrimaryRoad)]
        [InlineData("secondary", RoadTypeId.TwoLanesSecondaryRoad)]
        [InlineData("tertiary", RoadTypeId.TwoLanesSecondaryRoad)]
        [InlineData("road", RoadTypeId.TwoLanesSecondaryRoad)]
        [InlineData("living_street", RoadTypeId.TwoLanesConcreteRoad)]
        [InlineData("residential", RoadTypeId.TwoLanesConcreteRoad)]
        [InlineData("unclassified", RoadTypeId.TwoLanesConcreteRoad)]
        [InlineData("track", RoadTypeId.SingleLaneDirtPath)]
        [InlineData("service", RoadTypeId.SingleLaneConcreteRoad)]
        public void FromOSM_HighwayTypes_ReturnCorrectId(string highway, RoadTypeId expected)
        {
            var result = RoadTypeIdHelper.FromOSM(Tags(("highway", highway)));
            Assert.Equal(expected, result);
        }

        [Fact]
        public void FromOSM_UnknownHighway_ReturnsNull()
        {
            Assert.Null(RoadTypeIdHelper.FromOSM(Tags(("highway", "unknown_type"))));
        }

        [Fact]
        public void FromOSM_NoHighwayTag_ReturnsNull()
        {
            Assert.Null(RoadTypeIdHelper.FromOSM(new TagsCollection()));
        }

        [Fact]
        public void FromOSM_Footway_ReturnsConcrete()
        {
            var result = RoadTypeIdHelper.FromOSM(Tags(("highway", "footway")));
            Assert.Equal(RoadTypeId.ConcreteFootway, result);
        }

        [Fact]
        public void FromOSM_FootwaySidewalk_ReturnsNull()
        {
            var result = RoadTypeIdHelper.FromOSM(Tags(("highway", "footway"), ("footway", "sidewalk")));
            Assert.Null(result);
        }

        [Fact]
        public void FromOSM_FootwayCrossing_ReturnsNull()
        {
            var result = RoadTypeIdHelper.FromOSM(Tags(("highway", "footway"), ("footway", "crossing")));
            Assert.Null(result);
        }

        [Fact]
        public void FromOSM_PedestrianAsphalt_ReturnsConcrete()
        {
            var result = RoadTypeIdHelper.FromOSM(Tags(("highway", "pedestrian"), ("surface", "asphalt")));
            Assert.Equal(RoadTypeId.ConcreteFootway, result);
        }

        [Fact]
        public void FromOSM_PedestrianNonPaved_ReturnsTrail()
        {
            var result = RoadTypeIdHelper.FromOSM(Tags(("highway", "pedestrian")));
            Assert.Equal(RoadTypeId.Trail, result);
        }

        [Fact]
        public void GetSpecialSegment_ServiceWithPrivateAccess_ReturnsPrivateService()
        {
            var tags = Tags(("access", "private"));
            var result = RoadTypeIdHelper.GetSpecialSegment(RoadTypeId.SingleLaneConcreteRoad, tags);
            Assert.Equal(WaySpecialSegment.PrivateService, result);
        }

        [Fact]
        public void GetSpecialSegment_Driveway_ReturnsPrivateService()
        {
            var tags = Tags(("service", "driveway"));
            var result = RoadTypeIdHelper.GetSpecialSegment(RoadTypeId.SingleLaneConcreteRoad, tags);
            Assert.Equal(WaySpecialSegment.PrivateService, result);
        }

        [Fact]
        public void GetSpecialSegment_DrivewayPermissive_ReturnsNormal()
        {
            var tags = Tags(("service", "driveway"), ("motor_vehicle", "permissive"));
            var result = RoadTypeIdHelper.GetSpecialSegment(RoadTypeId.SingleLaneConcreteRoad, tags);
            Assert.Equal(WaySpecialSegment.Normal, result);
        }

        [Fact]
        public void GetSpecialSegment_Bridge_ReturnsBridge()
        {
            var tags = Tags(("bridge", "yes"));
            var result = RoadTypeIdHelper.GetSpecialSegment(RoadTypeId.TwoLanesPrimaryRoad, tags);
            Assert.Equal(WaySpecialSegment.Bridge, result);
        }

        [Fact]
        public void GetSpecialSegment_Embankment_ReturnsEmbankment()
        {
            var tags = Tags(("embankment", "yes"));
            var result = RoadTypeIdHelper.GetSpecialSegment(RoadTypeId.TwoLanesPrimaryRoad, tags);
            Assert.Equal(WaySpecialSegment.Embankment, result);
        }
    }
}
