namespace GameRealisticMap.Configuration
{
    /// <summary>
    /// Endpoints to data sources used by the application.
    /// </summary>
    public interface ISourceLocations
    {
        /// <summary>
        /// Endpoint to SRTM15+ elevation data (Pmad.Cartography format, formerly MapToolkit)
        /// </summary>
        Uri MapToolkitSRTM15Plus { get; }

        /// <summary>
        /// Endpoint to SRTM1 elevation data (Pmad.Cartography format, formerly MapToolkit)
        /// </summary>
        Uri MapToolkitSRTM1 { get; }

        /// <summary>
        /// Endpoint to AW3D30 elevation data (Pmad.Cartography format, formerly MapToolkit)
        /// </summary>
        Uri MapToolkitAW3D30 { get; }

        /// <summary>
        /// Endpoint to WeatherStats data, providing weather statistics data (e.g. ERA5AVG) used for climate generation.
        /// </summary>
        Uri WeatherStats { get; }

        /// <summary>
        /// Endpoint to Overpass API interpreter, used for querying OpenStreetMap data.
        /// </summary>
        Uri OverpassApiInterpreter { get; }

        /// <summary>
        /// Google map compatible WMTS endpoint to satellite image.
        /// Should have placeholders for {x}, {y}, and optionally for {z} (zoom level).
        /// </summary>
        Uri SatelliteImageProvider { get; }
    }
}
