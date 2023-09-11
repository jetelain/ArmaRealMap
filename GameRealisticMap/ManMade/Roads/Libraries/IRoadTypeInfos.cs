namespace GameRealisticMap.ManMade.Roads.Libraries
{
    public interface IRoadTypeInfos
    {
        RoadTypeId Id { get; }

        float Width { get; }

        float ClearWidth { get; }

        /// <summary>
        /// Generate street lamps (everywhere)
        /// </summary>
        StreetLampsCondition ProceduralStreetLamps { get; }

        /// <summary>
        /// Distance in meters between procedural street lamps
        /// </summary>
        float DistanceBetweenStreetLamps { get; }

        /// <summary>
        /// Generate sidewalks in Urban areas
        /// </summary>
        bool HasSideWalks { get; }
    }
}
