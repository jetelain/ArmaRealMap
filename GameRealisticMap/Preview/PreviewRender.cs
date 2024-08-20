using System.Text.Json;
using GameRealisticMap.Configuration;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Osm;
using GeoJSON.Text.Feature;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Preview
{
    public class PreviewRender
    {
        private readonly ITerrainArea terrainArea;
        private readonly IMapProcessingOptions imagery;
        private readonly IBuildersConfig config;
        private readonly ISourceLocations sources;

        public PreviewRender(ITerrainArea terrainArea, IMapProcessingOptions imagery)
            : this(terrainArea, imagery, new DefaultBuildersConfig(), new DefaultSourceLocations())
        {
        }

        public PreviewRender(ITerrainArea terrainArea, IMapProcessingOptions imagery, IBuildersConfig config, ISourceLocations sources)
        {
            this.terrainArea = terrainArea;
            this.imagery = imagery;
            this.config = config;
            this.sources = sources;
        }

        public async Task RenderHtml(IProgressScope scope, string targetFile, bool ignoreElevation = false)
        {
            try
            {
                Func<Type, bool>? filter = null;
                if (ignoreElevation)
                {
                    filter = (t) => t != typeof(ElevationContourData);
                }

                var catalog = new BuildersCatalog(config, sources);
                var count = catalog.CountOfType<IGeoJsonData>(filter);

                var loader = new OsmDataOverPassLoader(scope, sources);
                var osmSource = await loader.Load(terrainArea);
                if (scope.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var context = new BuildContext(catalog, scope, terrainArea, osmSource, imagery);
                var list = new List<Feature>();
                foreach (var data in catalog.GetOfType<IGeoJsonData>(context, filter))
                {
                    list.AddRange(data.ToGeoJson(p => p));
                    if (scope.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                var collection = new FeatureCollection(list);

                await RenderHtml(collection, targetFile);
            }
            catch (Exception ex)
            {
                scope.Failed(ex);
            }
            finally
            {
                scope.Dispose();
            }
        }

        public async Task RenderHtml<TData>(IProgressScope scope, string targetFile) 
            where TData: class, IGeoJsonData
        {
            try
            {
                var catalog = new BuildersCatalog(config, sources);

                var loader = new OsmDataOverPassLoader(scope, sources);
                var osmSource = await loader.Load(terrainArea);
                if (scope.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var context = new BuildContext(catalog, scope, terrainArea, osmSource, imagery);
                var list = new List<Feature>();
                var data = context.GetData<TData>();
                
                list.AddRange(data.ToGeoJson(p => p));
                if (scope.CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                var collection = new FeatureCollection(list);
                await RenderHtml(collection, targetFile);

            }
            catch (Exception ex)
            {
                scope.Failed(ex);
            }
            finally
            {
                scope.Dispose();
            }
        }

        //private IPosition Project(TerrainPoint point)
        //{
        //    var p = terrainArea.TerrainPointToLatLng(point);
        //    return new Position(p.Y, p.X);
        //}

        public static async Task RenderHtml(BuildContext context, string targetFile)
        {
            await RenderHtml(new FeatureCollection(context.GetOfType<IGeoJsonData>().SelectMany(b => b.ToGeoJson(p => p)).ToList()), targetFile);
        }

        public static async Task RenderHtml(FeatureCollection collection, string targetFile)
        {
            using (var reader = new StreamReader(typeof(PreviewRender).Assembly.GetManifestResourceStream("GameRealisticMap.Preview.grm-preview.js")!))
            {
                await File.WriteAllTextAsync(Path.Combine(Path.GetDirectoryName(targetFile)!,"grm-preview.js"), await reader.ReadToEndAsync());
            }

            using (var reader = new StreamReader(typeof(PreviewRender).Assembly.GetManifestResourceStream("GameRealisticMap.Preview.grm-preview.html")!))
            {
                var parts = (await reader.ReadToEndAsync()).Split(@"{""type"":""FeatureCollection""}");
                using var output = File.CreateText(targetFile);
                await output.WriteAsync(parts[0]);
                await output.FlushAsync();
                await JsonSerializer.SerializeAsync(output.BaseStream, collection);
                await output.WriteAsync(parts[1]);
            }
        }
    }
}
