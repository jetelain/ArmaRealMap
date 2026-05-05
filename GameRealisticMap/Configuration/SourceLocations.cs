using System.Text.Json;

namespace GameRealisticMap.Configuration
{
    /// <summary>
    /// Handles persistence of <see cref="ISourceLocations"/> to a JSON file,
    /// including automatic migration of outdated endpoint URLs.
    /// </summary>
    public static class SourceLocations
    {
        /// <summary>
        /// Default path of the JSON file used to persist source locations.
        /// Located in <c>%LOCALAPPDATA%\GameRealisticMap\sources.json</c>.
        /// </summary>
        public static string DefaultLocation { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap", "sources.json");

        /// <summary>
        /// Increment this constant whenever a new migration batch is added
        /// so that already-migrated files are not processed again.
        /// </summary>
        private const int CurrentConfigVersion = 1;

        /// <summary>
        /// Maps obsolete endpoint prefixes to their replacement prefixes.
        /// </summary>
        private static readonly (Uri OldPrefix, Uri NewPrefix)[] Migrations = new[]
        {
            (new Uri("https://dem.pmad.net/SRTM15Plus/"),        DefaultSourceLocations.Instance.MapToolkitSRTM15Plus),
            (new Uri("https://dem.pmad.net/SRTM1/"),             DefaultSourceLocations.Instance.MapToolkitSRTM1),
            (new Uri("https://dem.pmad.net/AW3D30/"),            DefaultSourceLocations.Instance.MapToolkitAW3D30),
            (new Uri("https://overpass-api.de/api/interpreter"), DefaultSourceLocations.Instance.OverpassApiInterpreter),
        };

        /// <summary>
        /// Replaces an obsolete URL prefix with its current equivalent.
        /// Returns the original <paramref name="uri"/> unchanged if no migration applies.
        /// </summary>
        private static Uri Migrate(Uri uri)
        {
            var str = uri.OriginalString;
            foreach (var (oldPrefix, newPrefix) in Migrations)
            {
                if (str.StartsWith(oldPrefix.OriginalString, StringComparison.OrdinalIgnoreCase))
                {
                    return new Uri(newPrefix.OriginalString + str.Substring(oldPrefix.OriginalString.Length));
                }
            }
            return uri;
        }

        /// <summary>
        /// Applies all endpoint migrations to <paramref name="json"/> when its
        /// <see cref="SourceLocationsJson.ConfigVersion"/> is below <see cref="CurrentConfigVersion"/>.
        /// </summary>
        private static SourceLocationsJson ApplyMigrations(SourceLocationsJson json)
        {
            if (json.ConfigVersion >= CurrentConfigVersion)
            {
                return json;
            }
            json.MapToolkitSRTM15Plus = Migrate(json.MapToolkitSRTM15Plus);
            json.MapToolkitSRTM1 = Migrate(json.MapToolkitSRTM1);
            json.MapToolkitAW3D30 = Migrate(json.MapToolkitAW3D30);
            json.OverpassApiInterpreter = Migrate(json.OverpassApiInterpreter);
            return json;
        }

        /// <summary>
        /// Loads source locations from <see cref="DefaultLocation"/>.
        /// Applies endpoint migrations when the stored config version is outdated.
        /// Returns <see cref="DefaultSourceLocations.Instance"/> when no file exists or deserialization fails.
        /// </summary>
        public static Task<ISourceLocations> Load()
        {
            return Load(DefaultLocation);
        }

        /// <summary>
        /// Loads source locations from <paramref name="path"/>.
        /// Applies endpoint migrations when the stored config version is outdated.
        /// Returns <see cref="DefaultSourceLocations.Instance"/> when no file exists or deserialization fails.
        /// </summary>
        public static async Task<ISourceLocations> Load(string path)
        {
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                var json = await JsonSerializer.DeserializeAsync<SourceLocationsJson>(stream);
                if (json != null)
                {
                    return ApplyMigrations(json);
                }
            }
            return DefaultSourceLocations.Instance;
        }

        /// <summary>
        /// Persists <paramref name="locations"/> to <see cref="DefaultLocation"/>
        /// and stamps the file with <see cref="CurrentConfigVersion"/>.
        /// </summary>
        public static Task Save(ISourceLocations locations)
        {
            return Save(DefaultLocation, locations);
        }

        /// <summary>
        /// Persists <paramref name="path"/> to <paramref name="location"/>
        /// and stamps the file with <see cref="CurrentConfigVersion"/>.
        /// </summary>
        public static async Task Save(string location, ISourceLocations path)
        {
            using var stream = File.Create(location);
            await JsonSerializer.SerializeAsync(stream, new SourceLocationsJson(
                path.MapToolkitSRTM15Plus,
                path.MapToolkitSRTM1,
                path.MapToolkitAW3D30,
                path.WeatherStats,
                path.OverpassApiInterpreter,
                null,
                path.SatelliteImageProvider,
                CurrentConfigVersion));
        }
    }
}
