namespace GameRealisticMap.Roads
{
    internal class DefaultRoadTypeInfos : IRoadTypeInfos
    {
        public DefaultRoadTypeInfos(RoadTypeId id, float width)
        {
            Id = id;
            Width = width;
        }

        public RoadTypeId Id { get; }
        public float Width { get; }
    }
}