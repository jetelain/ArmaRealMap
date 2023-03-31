namespace GameRealisticMap.ElevationModel
{
    internal class RawElevationData : ITerrainData
    {
        public RawElevationData(ElevationGrid rawElevation)
        {
            RawElevation = rawElevation;
        }

        public ElevationGrid RawElevation { get; }
    }
}
