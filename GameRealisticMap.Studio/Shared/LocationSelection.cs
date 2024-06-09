namespace GameRealisticMap.Studio.Shared
{
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
