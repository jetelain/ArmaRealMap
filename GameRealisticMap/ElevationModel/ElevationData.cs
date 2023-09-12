namespace GameRealisticMap.ElevationModel
{
    public class ElevationData
    {
        public ElevationData(ElevationGrid elevation)
        {
            Elevation = elevation;
        }

        public ElevationGrid Elevation { get; }
    }
}
