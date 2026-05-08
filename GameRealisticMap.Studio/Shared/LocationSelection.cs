namespace GameRealisticMap.Studio.Shared
{
    /// <summary>
    /// Represents a user-entered geographic location used when creating a new map configuration.
    /// Stores the raw coordinate string (WGS-84 lat/lng or UTM), whether it represents the
    /// map centre or south-west corner, and the resolved <see cref="ITerrainArea"/>.
    /// </summary>
    public sealed class LocationSelection
    {
        public LocationSelection(string coordinates, bool isCenter, ITerrainArea terrainArea)
        {
            Coordinates = coordinates;
            IsCenter = isCenter;
            TerrainArea = terrainArea;
        }

        public string Coordinates { get; }

        public bool IsCenter { get; }

        public ITerrainArea TerrainArea { get; }
    }
}
