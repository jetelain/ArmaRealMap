using CommandLine;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.CommandLine
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                return await Parser.Default.ParseArguments<GenerateObjectLayerOptions, GenerateWrpOptions, GenerateModOptions, GenerateTerrainBuilderOptions>(args)
                  .MapResult(
                    (GenerateObjectLayerOptions opts) => GenerateObjectLayer(opts),
                    (GenerateWrpOptions opts) => GenerateWrp(opts),
                    (GenerateModOptions opts) => GenerateMod(opts),
                    (GenerateTerrainBuilderOptions opts) => GenerateTerrainBuilder(opts),
                    errs => Task.FromResult(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return 2;
            }
        }

        private static async Task<int> GenerateTerrainBuilder(GenerateTerrainBuilderOptions opts)
        {
            using var workspace = await opts.CreateWorkspace();
            var generator = new Arma3TerrainBuilderGenerator(workspace.Assets, workspace.ProjectDrive, workspace.Sources);
            Directory.CreateDirectory(opts.TargetDirectory);
            await generator.GenerateTerrainBuilderFiles(workspace.Progress, workspace.MapConfig, opts.TargetDirectory);
            return 0;
        }

        private static async Task<int> GenerateObjectLayer(GenerateObjectLayerOptions opts)
        {
            using var workspace = await opts.CreateWorkspace();
            var generator = new Arma3TerrainBuilderGenerator(workspace.Assets, workspace.ProjectDrive, workspace.Sources);
            Directory.CreateDirectory(opts.TargetDirectory);
            await generator.GenerateOnlyOneLayer(workspace.Progress, workspace.MapConfig, opts.LayerName, opts.TargetDirectory);
            return 0;
        }

        private static async Task<int> GenerateWrp(GenerateWrpOptions opts)
        {
            using var workspace = await opts.CreateWorkspace();
            var generator = new Arma3MapGenerator(workspace.Assets, workspace.ProjectDrive, new NonePboCompilerFactory(), workspace.Sources);
            await generator.GenerateWrp(workspace.Progress, workspace.MapConfig, !opts.SkipPaa);
            return 0;
        }

        private static async Task<int> GenerateMod(GenerateModOptions opts)
        {
            if ( !OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Mod generation works only on Windows");
            }
            using var workspace = await opts.CreateWorkspace();
            var generator = new Arma3MapGenerator(workspace.Assets, workspace.ProjectDrive, new PboCompilerFactory(workspace.ProjectDrive), workspace.Sources);
            await generator.GenerateMod(workspace.Progress, workspace.MapConfig);
            return 0;
        }
    }
}