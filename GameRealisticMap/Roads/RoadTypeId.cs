namespace GameRealisticMap.Roads
{
    public enum RoadTypeId
    {
        /// <summary>
        /// 2x1 designed to be side by side
        /// </summary>
        TwoLanesMotorway = 1,

        /// <summary>
        /// 2x1
        /// </summary>
        TwoLanesPrimaryRoad = 2,

        TwoLanesSecondaryRoad = 3,

        TwoLanesConcreteRoad = 4,

        SingleLaneDirtRoad = 5,

        SingleLaneDirtPath = 6,

        Trail = 7 // TODO: Footpath in cities and in the wild does not look the same, make them different
    }
}