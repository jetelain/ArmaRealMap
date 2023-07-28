using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.GameLauncher;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Preview;
using GameRealisticMap.Satellite;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetConfigEditor;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;
using Gemini.Framework.Services;
using HugeImages.Storage;
using MapControl;
using Microsoft.Win32;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    internal class MapConfigEditorViewModel : PersistedDocument, IExplorerRootTreeItem
    {
        private readonly IShell _shell;
        private readonly IArma3DataModule _arma3DataModule;

        public MapConfigEditorViewModel(IShell shell, IArma3DataModule arma3DataModule)
        {
            _shell = shell;
            _arma3DataModule = arma3DataModule;
        }

        public List<string> BuiltinAssetConfigFiles { get; } = Arma3Assets.GetBuiltinList();

        public Arma3MapConfigJson Config { get; set; } = new Arma3MapConfigJson();

        public int[] GridSizes { get; } = new int[] { 256, 512, 1024, 2048, 4096, 8192 };
        
        public string Center
        {
            get { return Config.Center ?? string.Empty ; }
            set
            {
                Config.Center = value;
                if (!string.IsNullOrEmpty(value))
                {
                    Config.SouthWest = null;
                }
                NotifyOfPropertyChange(nameof(SouthWest));
                NotifyOfPropertyChange(nameof(Center));
                NotifyOfPropertyChange(nameof(Locations));
                IsDirty = true;
            }
        }

        public string SouthWest
        {
            get { return Config.SouthWest ?? string.Empty; }
            set
            {
                Config.SouthWest = value;
                if (!string.IsNullOrEmpty(value))
                {
                    Config.Center = null;
                }
                NotifyOfPropertyChange(nameof(SouthWest));
                NotifyOfPropertyChange(nameof(Center));
                NotifyOfPropertyChange(nameof(Locations));
                IsDirty = true;
            }
        }

        public float GridCellSize
        {
            get { return Config.GridCellSize; }
            set
            {
                Config.GridCellSize = NormalizeCellSize(value);
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridCellSize));
                NotifyOfPropertyChange(nameof(Locations));
                IsDirty = true;
            }
        }

        public int GridSize 
        { 
            get { return Config.GridSize; }
            set 
            {
                Config.GridSize = value;
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridSize));
                NotifyOfPropertyChange(nameof(Locations));
                IsDirty = true;
            }
        }

        public float MapSize 
        { 
            get { return Config.GridSize * Config.GridCellSize; }
            set
            {
                Config.GridSize = GridSizes.Max();
                foreach (var candidate in GridSizes)
                {
                    var cellsize = value / candidate;
                    if (cellsize > 2 && cellsize < 8)
                    {
                        Config.GridSize = candidate;
                        break;
                    }
                }
                Config.GridCellSize = NormalizeCellSize(value / Config.GridSize);
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridSize));
                NotifyOfPropertyChange(nameof(GridCellSize));
                NotifyOfPropertyChange(nameof(Locations));
                IsDirty = true;
            }
        }

        private float NormalizeCellSize(float v)
        {
            return MathF.Round(v * 8) / 8;
            // 0.125 precision
            // Map size is enforced as multiple of 32 meters
        }

        public IEnumerable<Location> Locations
        {
            get 
            { 
                if (!string.IsNullOrEmpty(Config.SouthWest) || !string.IsNullOrEmpty(Config.Center))
                {
                    var area = Config.ToArma3MapConfig().TerrainArea;
                    return area.TerrainBounds.Shell.Select(area.TerrainPointToLatLng).Select(l => new Location(l.Y, l.X));
                }
                return new List<Location>(); 
            }
        }

        public string AssetConfigFile
        {
            get { return Config.AssetConfigFile ?? string.Empty; }
            set
            {
                Config.AssetConfigFile = value;
                NotifyOfPropertyChange();
                IsDirty = true;
                _ = CheckDependencies();
            }
        }

        private async Task CheckDependencies()
        {
            var file = AssetConfigFile;
            MissingMods = new List<MissingMod>();
            if (!string.IsNullOrEmpty(file))
            {
                var fullpath = GetAssetFullPath(file);
                if (fullpath != null)
                {
                    try
                    {
                        var deps = await Arma3Assets.LoadDependenciesFromFile(fullpath);
                        MissingMods = await MissingMod.DetectMissingMods(_arma3DataModule, IoC.Get<IArma3ModsService>(), deps);
                    }
                    catch
                    {
                        // Ignore all errors at this stage
                    }
                }
            }
            NotifyOfPropertyChange(nameof(HasMissingMods));
            NotifyOfPropertyChange(nameof(MissingMods));
        }

        public bool HasMissingMods => MissingMods.Count > 0;
        public List<MissingMod> MissingMods { get; private set; } = new List<MissingMod>();

        public string TreeName => DisplayName;
        public string Icon => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/MapConfig.png";

        public override string DisplayName { get => base.DisplayName; set { base.DisplayName = value; NotifyOfPropertyChange(nameof(TreeName)); } }

        public IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        protected override async Task DoLoad(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            Config = await JsonSerializer.DeserializeAsync<Arma3MapConfigJson>(stream) ?? new Arma3MapConfigJson();
            NotifyOfPropertyChange(nameof(Config));
            NotifyOfPropertyChange(nameof(SouthWest));
            NotifyOfPropertyChange(nameof(Center));
            NotifyOfPropertyChange(nameof(MapSize));
            NotifyOfPropertyChange(nameof(GridSize));
            NotifyOfPropertyChange(nameof(GridCellSize));
            NotifyOfPropertyChange(nameof(Locations));
            NotifyOfPropertyChange(nameof(BoundingBox));
            NotifyOfPropertyChange(nameof(AssetConfigFile));
            await CheckDependencies();
        }

        protected override async Task DoNew()
        {
            Config = new Arma3MapConfigJson();
            var assetConfig = _shell.Documents.OfType<AssetConfigEditorViewModel>().Select(r => r.FilePath ?? r.FileName).FirstOrDefault();
            Config.AssetConfigFile = assetConfig ?? BuiltinAssetConfigFiles.FirstOrDefault() ?? string.Empty;
            NotifyOfPropertyChange(nameof(Config));
            NotifyOfPropertyChange(nameof(AssetConfigFile));
            await CheckDependencies();
        }

        protected override async Task DoSave(string filePath)
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, Config);
        }

        public Task GeneratePreview(bool ignoreElevation = false)
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GeneratePreview, t => DoGeneratePreview(t, ignoreElevation));
            return Task.CompletedTask;
        }

        public Task GeneratePreviewFast() => GeneratePreview(true);

        public Task GeneratePreviewNormal() => GeneratePreview(false);

        private async Task DoGeneratePreview(IProgressTaskUI task, bool ignoreElevation)
        {
            var a3config = Config.ToArma3MapConfig();
            var render = new PreviewRender(a3config.TerrainArea, a3config.Imagery);
            var target = Path.Combine(Path.GetTempPath(), "grm-preview.html");
            await render.RenderHtml(task, target, ignoreElevation);
            task.AddSuccessAction(() => ShellHelper.OpenUri(target), Labels.ViewResultInWebBrowser);
        }

        public Task ChooseAssetConfig()
        {
            var provider = IoC.Get<AssetConfigEditorProvider>();
            var dialog = new OpenFileDialog();
            dialog.Filter = string.Join("|", provider.FileTypes.Select(x => x.Name + "|*" + x.FileExtension));
            if (dialog.ShowDialog() == true)
            {
                AssetConfigFile = dialog.FileName;
            }
            return Task.CompletedTask;
        }

        public async Task EditAssetConfig()
        {
            var file = AssetConfigFile;
            if (!string.IsNullOrEmpty(file))
            {
                var doc = GetAssetConfigEditor(file);
                if (doc == null)
                {
                    var fullpath = GetAssetFullPath(file);
                    if (fullpath != null)
                    {
                        var provider = IoC.Get<AssetConfigEditorProvider>();

                        if (fullpath.StartsWith(Arma3Assets.BuiltinPrefix))
                        {
                            doc = (AssetConfigEditorViewModel)provider.Create();
                            var newName = Path.GetFileNameWithoutExtension(FileName) + "-assets.grma3a";
                            await provider.New(doc, newName);
                            await doc.CopyFrom(fullpath);
                            AssetConfigFile = newName;
                        }
                        else if (File.Exists(fullpath))
                        {
                            doc = (AssetConfigEditorViewModel)provider.Create();
                            await provider.Open(doc, fullpath);
                        }
                    }
                }
                if (doc != null)
                {
                    await _shell.OpenDocumentAsync(doc);
                }
            }
        }

        public string? GetAssetFullPath(string path)
        {
            if (path.StartsWith(Arma3Assets.BuiltinPrefix) || Path.IsPathRooted(path))
            {
                return path;
            }
            if (!IsNew)
            {
                var relPath = Path.Combine(Path.GetDirectoryName(FilePath)!, path);
                if (File.Exists(relPath))
                {
                    return Path.GetFullPath(relPath);
                }
            }
            return null;
        }

        public AssetConfigEditorViewModel? GetAssetConfigEditor(string file)
        {
            var fullpath = GetAssetFullPath(file);
            var editors = _shell.Documents.OfType<AssetConfigEditorViewModel>();
            if (fullpath != null)
            {
                return editors.FirstOrDefault(d => !d.IsNew && string.Equals(d.FilePath, fullpath, StringComparison.OrdinalIgnoreCase));
            }
            return editors.FirstOrDefault(d => d.IsNew && string.Equals(d.FileName, file, StringComparison.OrdinalIgnoreCase));
        }

        public Task CreateAssetConfig()
        {
            return Task.CompletedTask;
        }

        public Task GenerateMap()
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateMapForArma3, DoGenerateMap);
            return Task.CompletedTask;
        }

        public Task GenerateMod()
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateModForArma3, DoGenerateMod);

            return Task.CompletedTask;
        }
        

        private async Task DoGenerateMod(IProgressTaskUI task)
        {
            var a3config = Config.ToArma3MapConfig();

            ReportConfig(task, _arma3DataModule.ProjectDrive, a3config);

            var assets = await GetAssets(_arma3DataModule.Library, a3config);

            var generator = new Arma3MapGenerator(assets, _arma3DataModule.ProjectDrive, _arma3DataModule.CreatePboCompilerFactory());

            var name = await generator.GenerateMod(task, a3config);

            if (!string.IsNullOrEmpty(name))
            {
                if (name != a3config.WorldName)
                {
                    name = $"{name} - {a3config.WorldName}"; // use WorldName to make it unique
                }
                task.AddSuccessAction(() => ShellHelper.OpenUri(a3config.TargetModDirectory), Labels.ViewInFileExplorer);
                task.AddSuccessAction(() => ShellHelper.OpenUri("steam://run/107410"), Labels.OpenArma3Launcher, string.Format(Labels.OpenArma3LauncherWithGeneratedModHint, name));
                task.AddSuccessAction(() => Arma3Helper.Launch(assets.Dependencies, a3config.TargetModDirectory, a3config.WorldName), Labels.LaunchArma3, Labels.LaunchArma3Hint);
                await Arma3LauncherHelper.CreateLauncherPresetAsync(assets.Dependencies, a3config.TargetModDirectory, "GRM - " + name);
            }
        }

        private async Task DoGenerateMap(IProgressTaskUI task)
        {
            var a3config = Config.ToArma3MapConfig();

            ReportConfig(task, _arma3DataModule.ProjectDrive, a3config);

            var assets = await GetAssets(_arma3DataModule.Library, a3config);

            var generator = new Arma3MapGenerator(assets, _arma3DataModule.ProjectDrive, _arma3DataModule.CreatePboCompilerFactory());

            await generator.GenerateWrp(task, a3config);

            task.AddSuccessAction(() => ShellHelper.OpenUri(_arma3DataModule.ProjectDrive.GetFullPath(a3config.PboPrefix)), Labels.ViewInFileExplorer);
        }

        private async Task<Arma3Assets> GetAssets(ModelInfoLibrary library, Arma3MapConfig a3config)
        {
            var doc = GetAssetConfigEditor(a3config.AssetConfigFile);
            if (doc != null)
            {
                return doc.ToJson();
            }

            var file = GetAssetFullPath(a3config.AssetConfigFile);
            if (string.IsNullOrEmpty(file))
            {
                throw new ApplicationException(Labels.AssetConfigFileIsMissing);
            }

            return await Arma3Assets.LoadFromFile(library, file);
        }

        public Task GenerateSatMap()
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateSatelliteImage, (t) => DoImagery(t, s => s.CreateSatMap().OffloadAsync()));
            return Task.CompletedTask;
        }
        public Task GenerateRawSatMap()
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateRawSatelliteImage, DoRawSat);
            return Task.CompletedTask;
        }

        public Task GenerateIdMap()
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateIdMapImage, (t) => DoImagery(t, s => s.CreateIdMap().OffloadAsync()));
            return Task.CompletedTask;
        }

        private async Task DoImagery(IProgressTaskUI task, Func<IImagerySource,Task> action)
        {
            var projectDrive = _arma3DataModule.ProjectDrive;
            var library = _arma3DataModule.Library;

            var a3config = Config.ToArma3MapConfig();

            Arma3Assets assets = await GetAssets(library, a3config);

            var generator = new Arma3MapGenerator(assets, projectDrive, _arma3DataModule.CreatePboCompilerFactory());

            var target = Path.Combine(Path.GetTempPath(), a3config.WorldName);

            Directory.CreateDirectory(target);

            var source = await generator.GetImagerySource(task, a3config, new PersistentHugeImageStorage(target));
            if (source != null)
            {
                await action(source);
            }
            task.AddSuccessAction(() => ShellHelper.OpenUri(target), Labels.ViewInFileExplorer);
        }

        private async Task DoRawSat(IProgressTaskUI task)
        {
            var projectDrive = _arma3DataModule.ProjectDrive;
            var library = _arma3DataModule.Library;

            var a3config = Config.ToArma3MapConfig();

            Arma3Assets assets = await GetAssets(library, a3config);

            var generator = new Arma3MapGenerator(assets, projectDrive, _arma3DataModule.CreatePboCompilerFactory());

            var target = Path.Combine(Path.GetTempPath(), a3config.WorldName);
            Directory.CreateDirectory(target);

            var source = await generator.GetBuildContext(task, a3config, new PersistentHugeImageStorage(target));

            if (source != null)
            {
                await source.GetData<RawSatelliteImageData>().Image.OffloadAsync();
            }
            task.AddSuccessAction(() => ShellHelper.OpenUri(target), Labels.ViewInFileExplorer);
        }

        private void ReportConfig(IProgressTaskUI task, ProjectDrive projectDrive, Arma3MapConfig a3config)
        {
            task.WriteLine($"MountPath='{projectDrive.MountPath}'");
            if (projectDrive.SecondarySource is PboFileSystem pbo)
            {
                foreach(var path in pbo.GamePaths)
                {
                    task.WriteLine($"GamePaths+='{path}'");
                }
                foreach (var path in pbo.ModsPaths)
                {
                    task.WriteLine($"ModsPaths+='{path}'");
                }
            }
        }
    }
}
