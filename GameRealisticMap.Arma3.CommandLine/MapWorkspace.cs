using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Configuration;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.CommandLine
{
    internal class MapWorkspace : IDisposable
    {
        public MapWorkspace(ProjectDrive projectDrive, Arma3Assets assets, Arma3MapConfig a3config, ConsoleProgressSystem progress, ISourceLocations sources)
        {
            ProjectDrive = projectDrive;
            Assets = assets;
            MapConfig = a3config;
            Progress = progress;
            Sources = sources;
        }

        public ProjectDrive ProjectDrive { get; }
        public Arma3Assets Assets { get; }
        public Arma3MapConfig MapConfig { get; }
        public ConsoleProgressSystem Progress { get; }
        public ISourceLocations Sources { get; }

        public static async Task<MapWorkspace> Create(Arma3MapConfig a3config, string searchPath)
        {
            var progress = new ConsoleProgressSystem();

            using var init = progress.CreateStep("Initilization",1);

            var workspaceSettings = await WorkspaceSettings.Load();
            var projectDrive = workspaceSettings.CreateProjectDriveAutomatic();
            var models = new ModelInfoLibrary(projectDrive);

            var modelsCache = Path.Combine(searchPath, "modelinfo.json");
            if (!File.Exists(modelsCache))
            {
                modelsCache = ModelInfoLibrary.DefaultCachePath;
            }
            if (File.Exists(modelsCache))
            {
                await models.LoadFrom(modelsCache);
            }

            var assets = await Arma3Assets.LoadFromFile(models, a3config.AssetConfigFile);

            var sources = await SourceLocations.Load();

            return new MapWorkspace(projectDrive, assets, a3config, progress, sources);
        }

        public void Dispose()
        {
            Progress.DisplayReport();
        }
    }
}
