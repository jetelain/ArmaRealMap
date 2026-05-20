namespace GameRealisticMap.ElevationModel
{
    public sealed class RawElevationJson
    {
        public RawElevationJson(List<string> credits, ElevationMinMax[] outOfBounds)
        {
            Credits = credits;
            OutOfBounds = outOfBounds;
        }

        public List<string> Credits { get; }

        public ElevationMinMax[] OutOfBounds { get; }
    }
}
