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
        /// Minimal distance in meters between procedural street lamps
        /// </summary>
        float DistanceBetweenStreetLamps { get; }

        /// <summary>
        /// Maximal distance in meters between procedural street lamps
        /// </summary>
        float DistanceBetweenStreetLampsMax { get; }

        /// <summary>
        /// Generate sidewalks in Urban areas
        /// </summary>
        bool HasSideWalks { get; }
    }
}
