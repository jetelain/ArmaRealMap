namespace GameRealisticMap.ManMade.Roads.Libraries
{
    public interface IRoadTypeInfos
    {
        RoadTypeId Id { get; }

        float Width { get; }

        float ClearWidth { get; }

        bool HasStreetLamp { get; }

        float DistanceBetweenStreetLamps { get; }
    }
}
