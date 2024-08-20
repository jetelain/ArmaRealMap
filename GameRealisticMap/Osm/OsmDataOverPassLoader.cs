using GameRealisticMap.Configuration;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Osm
{
    public class OsmDataOverPassLoader : IOsmDataLoader
    {
        private readonly IProgressScope scope;
        private readonly ISourceLocations sources;
        private readonly string cacheDirectory = Path.Combine(Path.GetTempPath(), "GameRealisticMap", "OverPass");
        private readonly int cacheDays = 1;

        public OsmDataOverPassLoader(IProgressScope scope, ISourceLocations sources)
        {
            this.scope = scope;
            this.sources = sources;
        }

        public async Task<IOsmDataSource> Load(ITerrainArea area)
        {
            var box = new LatLngBounds(area);

            var cacheFileName = Path.Combine(cacheDirectory, FormattableString.Invariant($"{box.Name}.xml"));

            await DownloadFromOverPass(box, cacheFileName);

            using var report = scope.CreateSingle("Load OSM data");
            report.WriteLine($"Load {cacheFileName}");
            return OsmDataSource.CreateFromXml(cacheFileName);
        }

        private async Task DownloadFromOverPass(LatLngBounds box, string cacheFileName)
        {
            if (!File.Exists(cacheFileName) || (File.GetLastWriteTimeUtc(cacheFileName) < DateTime.UtcNow.AddDays(-cacheDays)))
            {
                using var report = scope.CreateSingle("Download from OSM");
                Directory.CreateDirectory(cacheDirectory);

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
                var uri = sources.OverpassApiInterpreter;
                report.WriteLine($"POST {uri.AbsoluteUri}");
                report.WriteLine($"{query}");
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
