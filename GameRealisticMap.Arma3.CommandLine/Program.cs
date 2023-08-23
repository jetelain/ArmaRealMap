using System.Text.Json;
using CommandLine;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.CommandLine
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                return await Parser.Default.ParseArguments<GenerateObjectLayerOptions>(args)
                  .MapResult(
                    (GenerateObjectLayerOptions opts) => GenerateObjectLayer(opts),
                    errs => Task.FromResult(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 2;
            }
        }

        private static async Task<int> GenerateObjectLayer(GenerateObjectLayerOptions opts)
        {
            var progress = new ConsoleProgressSystem();
            var workspaceSettings = await WorkspaceSettings.Load();
            var projectDrive = OperatingSystem.IsWindows() ? workspaceSettings.CreateProjectDrive() : workspaceSettings.CreateProjectDriveStandalone();
            var models = new ModelInfoLibrary(projectDrive);
            var a3config = await opts.GetConfigFile();
            var assets = await Arma3Assets.LoadFromFile(models, a3config.AssetConfigFile);
            var generator = new Arma3TerrainBuilderGenerator(assets, projectDrive);

            Directory.CreateDirectory(opts.TargetDirectory);

            await generator.GenerateOnlyOneLayer(progress, a3config, opts.LayerName, opts.TargetDirectory);

            return 0;
        }


    }
}