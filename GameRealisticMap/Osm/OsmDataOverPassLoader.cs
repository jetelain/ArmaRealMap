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
            var box = new OsmBounds(area);

            var cacheFileName = Path.Combine(cacheDirectory, FormattableString.Invariant($"{box.Left}_{box.Bottom}_{box.Right}_{box.Top}.xml"));

            using var report = progress.CreateStep("OSM", 2);

            await DownloadFromOverPass(box, cacheFileName);

            report.ReportOneDone();

            return OsmDataSource.CreateFromXml(cacheFileName);
        }

        private async Task DownloadFromOverPass(OsmBounds box, string cacheFileName)
        {
            if (!File.Exists(cacheFileName) || (File.GetLastWriteTimeUtc(cacheFileName) < DateTime.UtcNow.AddDays(-cacheDays)))
            {
                Directory.CreateDirectory(cacheDirectory);
                /*
                var lonWestern = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
                var latNorther = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
                var lonEastern = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
                var latSouthern = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
                var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={lonWestern},{latSouthern},{lonEastern},{latNorther}");
                */
                var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={box.Left},{box.Bottom},{box.Right},{box.Top}");
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
