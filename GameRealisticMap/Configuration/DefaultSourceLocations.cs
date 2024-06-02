namespace GameRealisticMap.Configuration
{
    public class DefaultSourceLocations : ISourceLocations
    {
        public Uri MapToolkitSRTM15Plus => new Uri("https://dem.pmad.net/SRTM15Plus/");

        public Uri MapToolkitSRTM1 => new Uri("https://dem.pmad.net/SRTM1/");

        public Uri MapToolkitAW3D30 => new Uri("https://dem.pmad.net/AW3D30/");

        public Uri WeatherStats => new Uri("https://weatherdata.pmad.net/ERA5AVG/");

        public Uri OverpassApiInterpreter => new Uri("https://overpass-api.de/api/interpreter");

        public Uri S2CloudlessBasePath => new Uri("https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/");
    }
}
