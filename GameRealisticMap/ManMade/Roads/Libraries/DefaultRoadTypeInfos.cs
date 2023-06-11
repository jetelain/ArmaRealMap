namespace GameRealisticMap.ManMade.Roads.Libraries
{
    internal class DefaultRoadTypeInfos : IRoadTypeInfos
    {
        public DefaultRoadTypeInfos(RoadTypeId id, float width, float clearWidth)
        {
            Id = id;
            Width = width;
            ClearWidth = clearWidth;
        }

        public RoadTypeId Id { get; }

        public float Width { get; }

        public float ClearWidth { get; }
    }
}