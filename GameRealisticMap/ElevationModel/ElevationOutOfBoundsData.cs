namespace GameRealisticMap.ElevationModel
{
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
