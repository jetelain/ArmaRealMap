namespace GameRealisticMap.Configuration
{
    public sealed class SourceLocationsJson : ISourceLocations
    {
        public required Uri MapToolkitSRTM15Plus { get; set; }
        public required Uri MapToolkitSRTM1 { get; set; }
        public required Uri MapToolkitAW3D30 { get; set; }
        public required Uri WeatherStats { get; set; }
        public required Uri OverpassApiInterpreter { get; set; }
        public required Uri S2CloudlessBasePath { get; set; }
    }
}
