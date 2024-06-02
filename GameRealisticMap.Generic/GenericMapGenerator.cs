using GameRealisticMap.Configuration;
using GameRealisticMap.Generic.Exporters;
using GameRealisticMap.Generic.Profiles;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap.Generic
{
    public class GenericMapGenerator
    {
        protected readonly ISourceLocations sources;

        public GenericMapGenerator(ISourceLocations sources)
        {
            this.sources = sources;
        }

        protected virtual async Task<IOsmDataSource> LoadOsmData(IProgressTask progress, GenericMapConfig config)
        {
            var loader = new OsmDataOverPassLoader(progress, sources);
            return await loader.Load(config.TerrainArea);
        }

        public async Task<IBuildContext?> GetBuildContext(IProgressTask progress, GenericMapConfig config, IHugeImageStorage hugeImageStorage)
        {
            var osmSource = await LoadOsmData(progress, config);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return null;
            }
            return CreateBuildContext(progress, config, osmSource, hugeImageStorage);
        }

        protected virtual BuildContext CreateBuildContext(IProgressTask progress, GenericMapConfig config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var builders = new BuildersCatalog(progress, new DefaultBuildersConfig(), sources);
            return new BuildContext(builders, progress, config.TerrainArea, osmSource, config, hugeImageStorage);
        }

        public async Task Generate(IProgressTask progress, GenericMapConfig config)
        {
            Directory.CreateDirectory(config.TargetDirectory);

            var profile = await ExportProfile.LoadFromFile(config.ExportProfileFile);

            progress.Total = profile.Entries.Count + 1;

            // Download from OSM
            var osmSource = await LoadOsmData(progress, config);
            if (progress.CancellationToken.IsCancellationRequested)
            {
                return;
            }
            progress.ReportOneDone();

            // Generate content
            var context = CreateBuildContext(progress, config, osmSource);

            await Process(progress, config, profile, context);

            context.DisposeHugeImages();
        }

        private static async Task Process(IProgressTask progress, GenericMapConfig config, ExportProfile profile, BuildContext context)
        {
            var exporters = new ExporterCatalog();
            foreach (var entry in profile.Entries)
            {
                using (progress.CreateScope(entry.Exporter))
                {
                    await exporters.Get(entry.Exporter).Export(Path.Combine(config.TargetDirectory, entry.FileName), entry.Format, context);
                }
                if (progress.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                progress.ReportOneDone();
            }
        }
    }
}
