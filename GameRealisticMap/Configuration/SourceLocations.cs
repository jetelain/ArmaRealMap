using System.Text.Json;

namespace GameRealisticMap.Configuration
{
    public static class SourceLocations
    {
        public static string DefaultLocation { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap", "sources.json");

        public static async Task<ISourceLocations> Load()
        {
            if (File.Exists(DefaultLocation))
            {
                using var stream = File.OpenRead(DefaultLocation);
                return await JsonSerializer.DeserializeAsync<SourceLocationsJson>(stream) ?? (ISourceLocations)DefaultSourceLocations.Instance;
            }
            return DefaultSourceLocations.Instance;
        }

        public static async Task Save(ISourceLocations locations)
        {
            using var stream = File.Create(DefaultLocation);
            await JsonSerializer.SerializeAsync(stream, new SourceLocationsJson(
                locations.MapToolkitSRTM15Plus,
                locations.MapToolkitSRTM1,
                locations.MapToolkitAW3D30,
                locations.WeatherStats,
                locations.OverpassApiInterpreter,
                null,
                locations.SatelliteImageProvider));
        }
    }
}
