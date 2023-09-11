namespace GameRealisticMap.ManMade.Roads.Libraries
{
    internal class DefaultRoadTypeInfos : IRoadTypeInfos
    {
        public DefaultRoadTypeInfos(RoadTypeId id, float width, float clearWidth, StreetLampsCondition proceduralStreetLamps = StreetLampsCondition.None, bool hasSideWalks = false)
        {
            Id = id;
            Width = width;
            ClearWidth = clearWidth;
            ProceduralStreetLamps = proceduralStreetLamps;
            HasSideWalks = hasSideWalks;
        }

        public RoadTypeId Id { get; }

        public float Width { get; }

        public float ClearWidth { get; }

        public StreetLampsCondition ProceduralStreetLamps { get; }

        public float DistanceBetweenStreetLamps => ClearWidth * 2.5f;

        public bool HasSideWalks { get; }
    }
}