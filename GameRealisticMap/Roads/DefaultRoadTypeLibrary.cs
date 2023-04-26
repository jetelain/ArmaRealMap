namespace GameRealisticMap.Roads
{
    public class DefaultRoadTypeLibrary : IRoadTypeLibrary
    {
        private readonly List<DefaultRoadTypeInfos> _roads = new List<DefaultRoadTypeInfos>()
        {
            new DefaultRoadTypeInfos(RoadTypeId.TwoLanesMotorway, 12f, 18f),
            new DefaultRoadTypeInfos(RoadTypeId.TwoLanesPrimaryRoad, 12f, 18f),

            new DefaultRoadTypeInfos(RoadTypeId.TwoLanesSecondaryRoad, 7.5f, 11.5f),
            new DefaultRoadTypeInfos(RoadTypeId.TwoLanesConcreteRoad, 7.5f, 11.5f),

            new DefaultRoadTypeInfos(RoadTypeId.SingleLaneConcreteRoad, 5.5f, 7.5f),
            new DefaultRoadTypeInfos(RoadTypeId.SingleLaneDirtRoad, 5.5f, 7.5f),

            new DefaultRoadTypeInfos(RoadTypeId.SingleLaneDirtPath, 4.5f, 6.5f),

            new DefaultRoadTypeInfos(RoadTypeId.ConcreteFootway, 1.5f, 1.5f),
            new DefaultRoadTypeInfos(RoadTypeId.Trail, 1.5f, 1.5f)
        };

        public IRoadTypeInfos GetInfo(RoadTypeId id)
        {
            return _roads.First(r => r.Id == id);
        }
    }
}
