﻿using System.Text.Json;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Preview
{
    public class PreviewRender
    {
        private readonly ITerrainArea terrainArea;
        private readonly IImageryOptions imagery;
        private readonly IRoadTypeLibrary<IRoadTypeInfos> library;

        public PreviewRender(ITerrainArea terrainArea, IImageryOptions imagery)
            : this(terrainArea, imagery, new DefaultRoadTypeLibrary())
        {
        }

        public PreviewRender(ITerrainArea terrainArea, IImageryOptions imagery, IRoadTypeLibrary<IRoadTypeInfos> library)
        {
            this.terrainArea = terrainArea;
            this.imagery = imagery;
            this.library = library;
        }

        public async Task RenderHtml(IProgressTask progress, string targetFile, bool ignoreElevation = false)
        {
            try
            {
                Func<Type, bool>? filter = null;
                if (ignoreElevation)
                {
                    filter = (t) => t != typeof(ElevationData);
                }

                var catalog = new BuildersCatalog(progress, library);
                var count = catalog.CountOfType<IGeoJsonData>(filter);
                progress.Total = count + 2;

                var loader = new OsmDataOverPassLoader(progress);
                var osmSource = await loader.Load(terrainArea);
                progress.ReportOneDone();
                if (progress.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var context = new BuildContext(catalog, progress, terrainArea, osmSource, imagery);
                var list = new List<Feature>();
                foreach (var data in catalog.GetOfType<IGeoJsonData>(context, filter))
                {
                    list.AddRange(data.ToGeoJson());
                    progress.ReportOneDone();
                    if (progress.CancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                var collection = new FeatureCollection(list);

                await RenderHtml(collection, targetFile);

                progress.ReportOneDone();
            }
            catch (Exception ex)
            {
                progress.Failed(ex);
            }
            finally
            {
                progress.Dispose();
            }
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