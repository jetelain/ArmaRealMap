using GameRealisticMap.Reporting;

namespace GameRealisticMap.Osm
{
    public class OsmDataOverPassLoader : IOsmDataLoader
    {
        private readonly IProgressSystem progress;
        private readonly string cacheDirectory = Path.Combine(Path.GetTempPath(), "GameRealisticMap", "OverPass");
        private readonly int cacheDays = 1;

        public OsmDataOverPassLoader(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public async Task<IOsmDataSource> Load(ITerrainArea area)
        {
            var box = new LatLngBounds(area);

            var cacheFileName = Path.Combine(cacheDirectory, FormattableString.Invariant($"{box.Name}.xml"));

            using var report = progress.CreateStep("OSM", 2);

            await DownloadFromOverPass(box, cacheFileName);

            report.ReportOneDone();

            progress.WriteLine($"Load {cacheFileName}");
            return OsmDataSource.CreateFromXml(cacheFileName);
        }

        private async Task DownloadFromOverPass(LatLngBounds box, string cacheFileName, double margin = 0.03)
        {
            if (!File.Exists(cacheFileName) || (File.GetLastWriteTimeUtc(cacheFileName) < DateTime.UtcNow.AddDays(-cacheDays)))
            {
                Directory.CreateDirectory(cacheDirectory);
                var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={box.Left - margin},{box.Bottom - margin},{box.Right + margin},{box.Top + margin}");
                progress.WriteLine($"Get {uri}");
                using (var client = new HttpClient())
                {
                    using (var target = File.Create(cacheFileName))
                    {
                        using (var download = await client.GetStreamAsync(uri))
                        {
                            await download.CopyToAsync(target);
                        }
                    }
                }
            }
        }
    }
}
