namespace GameRealisticMap.ManMade.Airports
{
    public sealed class AerowaysData
    {
        public AerowaysData(List<AirportsAeroways> airports)
        {
            Airports = airports;
        }

        public List<AirportsAeroways> Airports { get; }
    }
}
