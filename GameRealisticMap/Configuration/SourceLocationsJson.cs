namespace GameRealisticMap.Configuration
{
    public sealed class SourceLocationsJson : ISourceLocations
    {
        public SourceLocationsJson(
            Uri mapToolkitSRTM15Plus,
            Uri mapToolkitSRTM1,
            Uri mapToolkitAW3D30,
            Uri weatherStats,
            Uri overpassApiInterpreter,
            Uri? s2CloudlessBasePath = null,
            Uri? satelliteImageProvider = null)
        {
            MapToolkitSRTM15Plus = mapToolkitSRTM15Plus;
            MapToolkitSRTM1 = mapToolkitSRTM1;
            MapToolkitAW3D30 = mapToolkitAW3D30;
            WeatherStats = weatherStats;
            OverpassApiInterpreter = overpassApiInterpreter;
            S2CloudlessBasePath = s2CloudlessBasePath;

            if (satelliteImageProvider == null)
            {
                if (s2CloudlessBasePath != null)
                {
                    SatelliteImageProvider = new Uri(s2CloudlessBasePath.ToString().TrimEnd('/') + "/15/{y}/{x}.jpg");
                }
                else
                {
                    SatelliteImageProvider = DefaultSourceLocations.Instance.SatelliteImageProvider;
                }
            }
            else
            {
                SatelliteImageProvider = satelliteImageProvider;
            }
        }

        public Uri MapToolkitSRTM15Plus { get; set; }
        public Uri MapToolkitSRTM1 { get; set; }
        public Uri MapToolkitAW3D30 { get; set; }
        public Uri WeatherStats { get; set; }
        public Uri OverpassApiInterpreter { get; set; }
        public Uri? S2CloudlessBasePath { get; set; }
        public Uri SatelliteImageProvider { get; set; }
    }
}
