namespace GameRealisticMap.Configuration
{
    /// <summary>
    /// JSON-serializable implementation of <see cref="ISourceLocations"/>.
    /// The <see cref="ConfigVersion"/> field is used to track migration state.
    /// </summary>
    public sealed class SourceLocationsJson : ISourceLocations
    {
        /// <summary>
        /// Initializes a new instance with explicit endpoint URIs.
        /// </summary>
        /// <param name="mapToolkitSRTM15Plus">Base URI for SRTM15+ elevation tiles.</param>
        /// <param name="mapToolkitSRTM1">Base URI for SRTM1 elevation tiles.</param>
        /// <param name="mapToolkitAW3D30">Base URI for AW3D30 elevation tiles.</param>
        /// <param name="weatherStats">Base URI for weather statistics data.</param>
        /// <param name="overpassApiInterpreter">URI of the Overpass API interpreter endpoint.</param>
        /// <param name="s2CloudlessBasePath">Optional legacy base path for S2 Cloudless tiles; used to derive <see cref="SatelliteImageProvider"/> when <paramref name="satelliteImageProvider"/> is <c>null</c>.</param>
        /// <param name="satelliteImageProvider">Full URI template for satellite image tiles. Derived from <paramref name="s2CloudlessBasePath"/> or the default when <c>null</c>.</param>
        /// <param name="configVersion">Config schema version; defaults to <c>0</c> for files that pre-date versioning.</param>
        public SourceLocationsJson(
            Uri mapToolkitSRTM15Plus,
            Uri mapToolkitSRTM1,
            Uri mapToolkitAW3D30,
            Uri weatherStats,
            Uri overpassApiInterpreter,
            Uri? s2CloudlessBasePath = null,
            Uri? satelliteImageProvider = null,
            int configVersion = 0)
        {
            ConfigVersion = configVersion;
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

        /// <summary>
        /// Config schema version stored in the JSON file.
        /// A value of <c>0</c> means the file predates versioning and may need migration.
        /// </summary>
        public int ConfigVersion { get; init; }

        public Uri MapToolkitSRTM15Plus { get; set; }
        public Uri MapToolkitSRTM1 { get; set; }
        public Uri MapToolkitAW3D30 { get; set; }
        public Uri WeatherStats { get; set; }
        public Uri OverpassApiInterpreter { get; set; }
        public Uri? S2CloudlessBasePath { get; set; }
        public Uri SatelliteImageProvider { get; set; }
    }
}
