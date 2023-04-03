using System.Text.Json;
using GameRealisticMap.Buildings;
using GameRealisticMap.Nature;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GameRealisticMap.Roads;
using GameRealisticMap.Water;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var progress = new ConsoleProgressSystem();

            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8271", 2.5f, 512);

            var loader = new OsmDataOverPassLoader(progress);

            var osmSource = await loader.Load(area);

            var context = new BuildContext(area, osmSource);

            context.RegisterAll(progress, new DefaultRoadTypeLibrary());

            context.GetData<RoadsData>();
            context.GetData<BuildingsData>();
            context.GetData<WaterData>();
            context.GetData<ForestData>();
            context.GetData<ScrubData>();
            context.GetData<RocksData>();

            var collection = new FeatureCollection(context.ComputedData.SelectMany(d => d.ToGeoJson()).ToList());

            var json = JsonSerializer.Serialize(collection);

            using(var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("GameRealisticMap.CommandLine.preview.html")))
            {
                File.WriteAllText("preview.html",reader.ReadToEnd().Replace(@"{""type"":""FeatureCollection""}", json));
            }
            using (var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("GameRealisticMap.CommandLine.preview.js")))
            {
                File.WriteAllText("preview.js", reader.ReadToEnd());
            }

        }
    }
}