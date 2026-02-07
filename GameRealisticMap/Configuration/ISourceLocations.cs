namespace GameRealisticMap.Configuration
{
    public interface ISourceLocations
    {
        Uri MapToolkitSRTM15Plus { get; }

        Uri MapToolkitSRTM1 { get; }

        Uri MapToolkitAW3D30 { get; }

        Uri WeatherStats { get; }

        Uri OverpassApiInterpreter { get; }

        Uri SatelliteImageProvider { get; }
    }
}
