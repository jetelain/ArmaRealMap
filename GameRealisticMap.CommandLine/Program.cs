using System.Text.Json;
using GameRealisticMap.Buildings;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Nature;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Satellite;
using GameRealisticMap.Water;
using GeoJSON.Text.Feature;
using MapToolkit.Drawing;

namespace GameRealisticMap.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var progress = new ConsoleProgressSystem();

            var catalog = new BuildersCatalog(progress, new DefaultRoadTypeLibrary());

            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8271", 2.5f, 1024);

            var loader = new OsmDataOverPassLoader(progress);

            var osmSource = await loader.Load(area);

            var context = new BuildContext(catalog, progress, area, osmSource);
            
            context.GetData<RawElevationData>();
            context.GetData<ElevationData>();
            context.GetData<RoadsData>();
            context.GetData<BuildingsData>();
            context.GetData<WaterData>();
            context.GetData<ForestData>();
            context.GetData<ScrubData>();
            context.GetData<RocksData>();
            context.GetData<ForestRadialData>();
            context.GetData<ScrubRadialData>();
            
            var all = catalog.GetAll(context);

            var collection = new FeatureCollection(all.SelectMany(d => d.ToGeoJson()).ToList());

            var json = JsonSerializer.Serialize(collection);

            using(var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("GameRealisticMap.CommandLine.preview.html")))
            {
                File.WriteAllText("preview.html",reader.ReadToEnd().Replace(@"{""type"":""FeatureCollection""}", json));
            }
            using (var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("GameRealisticMap.CommandLine.preview.js")))
            {
                File.WriteAllText("preview.js", reader.ReadToEnd());
            }

            //var x = JsonSerializer.Serialize(ImageTiler.DefaultToWebp(context.GetData<RawSatelliteImageData>().Image, "rawsat"));

        }
    }
}