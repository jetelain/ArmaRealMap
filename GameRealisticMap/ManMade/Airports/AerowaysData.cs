using System.Text.Json.Serialization;

namespace GameRealisticMap.ManMade.Airports
{
    public sealed class AerowaysData
    {
        public AerowaysData(List<AirportAeroways> insideAirports, List<Aeroway> outsideAirports)
        {
            InsideAirports = insideAirports;
            OutsideAirports = outsideAirports;
        }

        public List<AirportAeroways> InsideAirports { get; }

        public List<Aeroway> OutsideAirports { get; }

        [JsonIgnore]
        public IEnumerable<Aeroway> Aeroways => OutsideAirports.Concat(InsideAirports.SelectMany(a => a.Aeroways));

    }
}
