using System.Text.Json;
using GameRealisticMap.Configuration;

namespace GameRealisticMap.Test.Configuration
{
    public class SourceLocationsTest
    {
        // ── helpers ──────────────────────────────────────────────────────────

        private static SourceLocationsJson RoundTrip(SourceLocationsJson original)
        {
            var json = JsonSerializer.Serialize(original);
            return JsonSerializer.Deserialize<SourceLocationsJson>(json)!;
        }

        private static SourceLocationsJson MakeDefaults(int configVersion = 1)
        {
            var d = DefaultSourceLocations.Instance;
            return new SourceLocationsJson(
                d.MapToolkitSRTM15Plus,
                d.MapToolkitSRTM1,
                d.MapToolkitAW3D30,
                d.WeatherStats,
                d.OverpassApiInterpreter,
                configVersion: configVersion);
        }

        // ── ConfigVersion round-trip ─────────────────────────────────────────

        [Fact]
        public void ConfigVersion_IsSerializedAndDeserialized()
        {
            var original = MakeDefaults(configVersion: 1);
            var restored = RoundTrip(original);
            Assert.Equal(1, restored.ConfigVersion);
        }

        [Fact]
        public void ConfigVersion_DefaultsToZeroWhenAbsentFromJson()
        {
            // JSON without a configVersion field
            const string json = """
                {
                    "mapToolkitSRTM15Plus": "https://cdn.dem.pmad.net/SRTM15Plus/",
                    "mapToolkitSRTM1":      "https://cdn.dem.pmad.net/SRTM1/",
                    "mapToolkitAW3D30":     "https://cdn.dem.pmad.net/AW3D30/",
                    "weatherStats":         "https://weatherdata.pmad.net/ERA5AVG/",
                    "overpassApiInterpreter":"https://overpass.pmad.net/api/interpreter",
                    "satelliteImageProvider":"https://tiles.maps.eox.at/wmts/1.0.0/s2cloudless-2020_3857/default/GoogleMapsCompatible/15/{y}/{x}.jpg"
                }
                """;

            var restored = JsonSerializer.Deserialize<SourceLocationsJson>(json)!;
            Assert.Equal(0, restored.ConfigVersion);
        }

        // ── Migration: old → new endpoints ──────────────────────────────────

        [Theory]
        [InlineData("https://dem.pmad.net/SRTM15Plus/",  "https://cdn.dem.pmad.net/SRTM15Plus/")]
        [InlineData("https://dem.pmad.net/SRTM1/",       "https://cdn.dem.pmad.net/SRTM1/")]
        [InlineData("https://dem.pmad.net/AW3D30/",      "https://cdn.dem.pmad.net/AW3D30/")]
        [InlineData("https://overpass-api.de/api/interpreter", "https://overpass.pmad.net/api/interpreter")]
        public async Task Load_MigratesOldEndpoints(string oldUrl, string expectedUrl)
        {
            var d = DefaultSourceLocations.Instance;

            // Build a JSON whose version is 0 and one of the endpoints is the old URL
            var source = new SourceLocationsJson(
                mapToolkitSRTM15Plus:    oldUrl.Contains("SRTM15") ? new Uri(oldUrl) : d.MapToolkitSRTM15Plus,
                mapToolkitSRTM1:         oldUrl.Contains("SRTM1/") ? new Uri(oldUrl) : d.MapToolkitSRTM1,
                mapToolkitAW3D30:        oldUrl.Contains("AW3D30") ? new Uri(oldUrl) : d.MapToolkitAW3D30,
                weatherStats:            d.WeatherStats,
                overpassApiInterpreter:  oldUrl.Contains("overpass") ? new Uri(oldUrl) : d.OverpassApiInterpreter,
                configVersion: 0);

            var path = Path.GetTempFileName();
            try
            {
                await using (var stream = File.Create(path))
                    await JsonSerializer.SerializeAsync(stream, source);

                // Temporarily redirect DefaultLocation by reading the file directly
                // (same code path as SourceLocations.Load but with a custom path)
                await using var readStream = File.OpenRead(path);
                var json = await JsonSerializer.DeserializeAsync<SourceLocationsJson>(readStream);
                Assert.NotNull(json);

                // Invoke the migration via the internal helper through round-trip Save/Load
                // by saving to a temp location and reloading.
                var migrated = await SourceLocations.Load(path);

                var actual = oldUrl.Contains("SRTM15") ? migrated.MapToolkitSRTM15Plus
                           : oldUrl.Contains("SRTM1/") ? migrated.MapToolkitSRTM1
                           : oldUrl.Contains("AW3D30") ? migrated.MapToolkitAW3D30
                           : migrated.OverpassApiInterpreter;

                Assert.Equal(new Uri(expectedUrl), actual);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task Load_DoesNotMigrateWhenVersionIsCurrent()
        {
            // Already-migrated file: version = 1, endpoints are already new
            var d = DefaultSourceLocations.Instance;
            var source = MakeDefaults(configVersion: 1);

            var path = Path.GetTempFileName();
            try
            {
                await using (var stream = File.Create(path))
                    await JsonSerializer.SerializeAsync(stream, source);

                var loaded = await SourceLocations.Load(path);

                Assert.Equal(d.MapToolkitSRTM15Plus, loaded.MapToolkitSRTM15Plus);
                Assert.Equal(d.MapToolkitSRTM1,      loaded.MapToolkitSRTM1);
                Assert.Equal(d.MapToolkitAW3D30,     loaded.MapToolkitAW3D30);
                Assert.Equal(d.OverpassApiInterpreter, loaded.OverpassApiInterpreter);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task Load_ReturnsDefaultWhenFileDoesNotExist()
        {
            var loaded = await SourceLocations.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));
            Assert.Same(DefaultSourceLocations.Instance, loaded);
        }

        // ── Save stamps CurrentConfigVersion ────────────────────────────────

        [Fact]
        public async Task Save_StampsCurrentConfigVersion()
        {
            var path = Path.GetTempFileName();
            try
            {
                await SourceLocations.Save(path, DefaultSourceLocations.Instance);

                await using var stream = File.OpenRead(path);
                var json = await JsonSerializer.DeserializeAsync<SourceLocationsJson>(stream);
                Assert.NotNull(json);
                Assert.True(json!.ConfigVersion > 0, "ConfigVersion should be stamped as > 0 after Save.");
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
