namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Final processed elevation data with all terrain constraints applied:
    /// roads have correct embankment/bridge transitions, watercourses flow downhill,
    /// and lakes are flat at their water level.
    /// This is the elevation grid written into the Arma 3 WRP file.
    /// </summary>
    public class ElevationData
    {
        public ElevationData(ElevationGrid elevation)
        {
            Elevation = elevation;
        }

        public ElevationGrid Elevation { get; }
    }
}
