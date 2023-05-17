using System.Diagnostics;
using System.Text.Json;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Arma3.CommandLine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var projectDrive = new ProjectDrive(Arma3ToolsHelper.GetProjectDrivePath(), new PboFileSystem());

            projectDrive.AddMountPoint(@"z\arm\addons", @"C:\Users\Julien\source\repos\ArmaRealMap\PDrive\z\arm\addons");

            var a3config = new Arma3MapConfigJson()
            {
                SouthWest = "47.6856, 6.8270",
                GridSize = 1024,
                GridCellSize = 2.5f,
                AssetConfigFile = @"C:\Users\Julien\source\repos\ArmaRealMap\ArmToGrmA3\bin\Debug\net6.0\CentralEurope.json",
                TileSize = 512
            }.ToArma3MapConfig();


            var progress = new ConsoleProgressSystem();

            var library = new ModelInfoLibrary(projectDrive);

            var assets = await Arma3Assets.LoadFromFile(library, a3config.AssetConfigFile);

            var sw = Stopwatch.StartNew();

            var catalog = new BuildersCatalog(progress, assets);

            var loader = new OsmDataOverPassLoader(progress);

            var osmSource = await loader.Load(a3config.TerrainArea);

            var context = new BuildContext(catalog, progress, a3config.TerrainArea, osmSource, a3config.Imagery);
            
            var generator = new Arma3MapGenerator(assets, progress, projectDrive, projectDrive);

            generator.WriteDirectlyWrp(a3config, context, a3config.TerrainArea);

            await projectDrive.ProcessImageToPaa(progress);

            sw.Stop();

            var surface = a3config.SizeInMeters * a3config.SizeInMeters / 1000000d;
            
            Console.WriteLine($"It took {sw.ElapsedMilliseconds} msec for {surface:0.0} Km², {sw.ElapsedMilliseconds / surface:0} msec/Km²");

            var all = catalog.GetOfType<IGeoJsonData>(context).SelectMany(d => d.ToGeoJson()).ToList();
            //var preview = new ModelPreviewHelper(library);
            //var all = preview.ToGeoJson(new ForestGenerator(progress, assets).Generate(a3config, context)).ToList();
            var json = JsonSerializer.Serialize(new FeatureCollection(all));
            using (var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("GameRealisticMap.Arma3.CommandLine.preview.html")!))
            {
                File.WriteAllText("preview.html", reader.ReadToEnd().Replace(@"{""type"":""FeatureCollection""}", json));
            }
            using (var reader = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("GameRealisticMap.Arma3.CommandLine.preview.js")!))
            {
                File.WriteAllText("preview.js", reader.ReadToEnd());
            }

            await Arma3ToolsHelper.BuildWithMikeroPboProject(a3config.PboPrefix, @"C:\temp\@ArmTest");
        }

    }
}