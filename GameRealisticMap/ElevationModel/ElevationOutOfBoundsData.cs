namespace GameRealisticMap.ElevationModel
{
    /// <summary>
    /// Minimum and maximum elevation values for the terrain area just outside each edge of the grid.
    /// Used to blend the terrain seamlessly with surrounding topography when extending the WRP edges.
    /// </summary>
    public class ElevationOutOfBoundsData
    {
        public const int Distance = 750;

        public ElevationOutOfBoundsData(ElevationMinMax[] outOfBounds)
        {
            OutOfBounds = outOfBounds;
        }

        public ElevationMinMax[] OutOfBounds { get; }
    }
}
