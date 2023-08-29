namespace GameRealisticMap.ManMade.Roads.Libraries
{
    internal class DefaultRoadTypeInfos : IRoadTypeInfos
    {
        public DefaultRoadTypeInfos(RoadTypeId id, float width, float clearWidth, bool hasStreetLamp = false)
        {
            Id = id;
            Width = width;
            ClearWidth = clearWidth;
            HasStreetLamp = hasStreetLamp;
        }

        public RoadTypeId Id { get; }

        public float Width { get; }

        public float ClearWidth { get; }

        public bool HasStreetLamp { get; }

        public float DistanceBetweenStreetLamps => ClearWidth * 2.5f;
    }
}