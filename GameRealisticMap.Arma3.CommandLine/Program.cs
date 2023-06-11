using System.Diagnostics;
using System.Text.Json;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Osm;
using GameRealisticMap.Preview;
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

            var generator = new Arma3MapGenerator(assets, projectDrive);

            var context = await generator.GenerateWrp(progress, a3config);

            sw.Stop();

            context?.DisposeHugeImages();

            var surface = a3config.SizeInMeters * a3config.SizeInMeters / 1000000d;

            Console.WriteLine($"It took {sw.ElapsedMilliseconds} msec for {surface:0.0} Km², {sw.ElapsedMilliseconds / surface:0} msec/Km²");

            var all = context.Catalog.GetOfType<IGeoJsonData>(context).SelectMany(d => d.ToGeoJson(p => p)).ToList();

            await PreviewRender.RenderHtml(new FeatureCollection(all), Path.GetFullPath("preview.html"));

            await Arma3ToolsHelper.BuildWithMikeroPboProject(a3config.PboPrefix, @"C:\temp\@ArmTest", progress);
        }
    }
}