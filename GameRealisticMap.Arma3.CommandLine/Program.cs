using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
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

            var models = new ModelInfoLibrary(projectDrive);

            var generator = new Arma3DemoMapGenerator(await Arma3Assets.LoadFromFile(models,"builtin:CentralEurope.grma3a"), projectDrive, "CentralEurope");

            var config = await generator.GenerateMod(new ConsoleProgressSystem());
        }
    }
}