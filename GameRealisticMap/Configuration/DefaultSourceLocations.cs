namespace GameRealisticMap.Configuration
{
    /// <summary>
    /// Default values for <see cref="ISourceLocations"/>, used if no user configuration file is found.
    /// </summary>
    public class DefaultSourceLocations : ISourceLocations
    {
        public static readonly ISourceLocations Instance = new DefaultSourceLocations();

        public Uri MapToolkitSRTM15Plus => new Uri("https://cdn.dem.pmad.net/SRTM15Plus/"); // Previously https://dem.pmad.net/SRTM15Plus/

        public Uri MapToolkitSRTM1 => new Uri("https://cdn.dem.pmad.net/SRTM1/"); // Previously https://dem.pmad.net/SRTM1/

        public Uri MapToolkitAW3D30 => new Uri("https://cdn.dem.pmad.net/AW3D30/"); // Previously https://dem.pmad.net/AW3D30/

        public Uri WeatherStats => new Uri("https://weatherdata.pmad.net/ERA5AVG/");

        public Uri OverpassApiInterpreter => new Uri("https://overpass.pmad.net/api/interpreter"); // Previously https://overpass-api.de/api/interpreter

        public Uri SatelliteImageProvider => new Uri("https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/15/{y}/{x}.jpg");
    }
}
