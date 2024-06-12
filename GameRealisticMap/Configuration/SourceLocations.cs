using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
                return await JsonSerializer.DeserializeAsync<SourceLocationsJson>(stream) ?? (ISourceLocations)new DefaultSourceLocations();
            }
            return new DefaultSourceLocations();
        }

        public static async Task Save(ISourceLocations locations)
        {
            using var stream = File.Create(DefaultLocation);
            await JsonSerializer.SerializeAsync(stream, new SourceLocationsJson() { 
                 MapToolkitAW3D30 = locations.MapToolkitAW3D30, 
                 MapToolkitSRTM1 = locations.MapToolkitSRTM1,
                 MapToolkitSRTM15Plus = locations.MapToolkitSRTM15Plus,
                 OverpassApiInterpreter = locations.OverpassApiInterpreter,
                 S2CloudlessBasePath = locations.S2CloudlessBasePath,
                 WeatherStats = locations.WeatherStats
             });
        }
    }
}
