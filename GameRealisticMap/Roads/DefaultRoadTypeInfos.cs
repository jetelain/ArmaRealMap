namespace GameRealisticMap.Roads
{
    internal class DefaultRoadTypeInfos : IRoadTypeInfos
    {
        public DefaultRoadTypeInfos(RoadTypeId id, float width)
        {
            Id = id;
            Width = width;

            if (Id < RoadTypeId.TwoLanesPrimaryRoad)
            {
                ClearWidth = Width + 6f;
            }
            else if (Id < RoadTypeId.SingleLaneDirtPath)
            {
                ClearWidth = Width + 4f;
            }
            else if (Id < RoadTypeId.Trail)
            {
                ClearWidth = Width + 2f;
            }
            else
            {
                ClearWidth = width;
            }
        }

        public RoadTypeId Id { get; }

        public float Width { get; }

        public float ClearWidth { get; }
    }
}