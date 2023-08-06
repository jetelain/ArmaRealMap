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

            await DownloadFromOverPass(box, cacheFileName);

            using var report = progress.CreateStep("Load OSM data", 1);
            progress.WriteLine($"Load {cacheFileName}");
            return OsmDataSource.CreateFromXml(cacheFileName);
        }

        private async Task DownloadFromOverPass(LatLngBounds box, string cacheFileName)
        {
            if (!File.Exists(cacheFileName) || (File.GetLastWriteTimeUtc(cacheFileName) < DateTime.UtcNow.AddDays(-cacheDays)))
            {
                using var report = progress.CreateStep("Download from OSM", 1);
                Directory.CreateDirectory(cacheDirectory);
                //double margin = 0.03;
                //var uri = FormattableString.Invariant($"https://overpass-api.de/api/map?bbox={box.Left - margin},{box.Bottom - margin},{box.Right + margin},{box.Top + margin}");
                //progress.WriteLine($"GET {uri}");
                //using (var client = new HttpClient())
                //{
                //    using (var target = File.Create(cacheFileName))
                //    {
                //        using (var download = await client.GetStreamAsync(uri))
                //        {
                //            await download.CopyToAsync(target);
                //        }
                //    }
                //}

                // https://wiki.openstreetmap.org/wiki/Overpass_API/Overpass_API_by_Example#Example:_Simplest_possible_map_call

                var qlbb = FormattableString.Invariant($"{box.Bottom},{box.Left},{box.Top},{box.Right}");
                var query = FormattableString.Invariant(@$"[timeout:300];
(
  node({qlbb});
  rel(bn)->.x;
  way(bn);
  rel(bw);
);
(
  ._;
  way(r);
);
(
  ._;
  node(r)->.x;
  node(w);
);
(
  ._;
  rel(br);
  rel(br);
  rel(br);
  rel(br);
);
out;"); ;
                var uri = "https://overpass-api.de/api/interpreter";
                progress.WriteLine($"POST {uri}");
                progress.WriteLine($"{query}");
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(300);
                    using var download = await client.PostAsync(uri, new FormUrlEncodedContent(new Dictionary<string, string>() { { "data", query } }));
                    using var stream = await download.Content.ReadAsStreamAsync(); 
                    using (var target = File.Create(cacheFileName))
                    {
                        await stream.CopyToAsync(target);
                    }
                }
            }
        }
    }
}
