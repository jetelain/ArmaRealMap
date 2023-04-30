using System.Text.Json;
using GameRealisticMap.IO;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var progress = new ConsoleProgressSystem();

            var catalog = new BuildersCatalog(progress, new DefaultRoadTypeLibrary());

            var area = TerrainAreaUTM.CreateFromSouthWest("47.6856, 6.8270", 2.5f, 1024/*8*/);

            var loader = new OsmDataOverPassLoader(progress);

            var osmSource = await loader.Load(area);

            var context = new BuildContext(catalog, progress, area, osmSource);

            var serializer = new ContextSerializer(catalog);

            await serializer.WriteToDirectory("test", context);

 
            var all = catalog.GetOfType<IGeoJsonData>(context);

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

            // new HillshaderFast(new MapToolkit.Vector(2.5, 2.5)).GetPixelsAlphaBelowFlat(context.GetData<ElevationData>().Elevation.ToDataCell()).SaveAsPng("elevation.png");

        }
    }
}