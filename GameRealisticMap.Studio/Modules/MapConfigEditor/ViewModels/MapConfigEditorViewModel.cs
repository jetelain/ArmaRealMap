﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.GameLauncher;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Configuration;
using GameRealisticMap.Preview;
using GameRealisticMap.Satellite;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor;
using GameRealisticMap.Studio.Modules.AssetConfigEditor;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels;
using GameRealisticMap.Studio.Modules.Main;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework.Services;
using MapControl;
using Microsoft.Win32;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    internal class MapConfigEditorViewModel : MapConfigEditorBase, IExplorerRootTreeItem, IMainDocument
    {
        private readonly IArma3DataModule _arma3DataModule;
        private bool _showPreview;

        public MapConfigEditorViewModel(IShell shell, IArma3DataModule arma3DataModule)
            : base(shell)
        {
            _arma3DataModule = arma3DataModule;
        }

        public List<string> BuiltinAssetConfigFiles { get; } = Arma3Assets.GetBuiltinList();

        public Arma3MapConfigJson Config { get; set; } = new Arma3MapConfigJson() { UseColorCorrection = true, PrivateServiceRoadThreshold = MapProcessingOptions.Default.PrivateServiceRoadThreshold };

        public override int[] GridSizes => GridHelper.Arma3GridSizes;

        public int[] IdMapMultipliers => Arma3MapConfig.ValidIdMapMultipliers;

        public override string Center
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
                NotifyCoordinatesRelated();
                IsDirty = true;
            }
        }

        private void NotifyCoordinatesRelated()
        {
            NotifyOfPropertyChange(nameof(MapSelection));

            UpdateAutomaticValues();
        }

        private void UpdateAutomaticValues()
        {
            AutomaticWorldName = string.Empty;
            AutomaticPboPrefix = string.Empty;
            AutomaticTargetModDirectory = string.Empty;
            AutomaticTileSize = 512;
            if (!string.IsNullOrEmpty(Center) || !string.IsNullOrEmpty(SouthWest))
            {
                try
                {
                    var config = Config.ToArma3MapConfig();
                    AutomaticWorldName = config.WorldName;
                    AutomaticPboPrefix = config.PboPrefix;
                    AutomaticTargetModDirectory = config.TargetModDirectory;
                    AutomaticTileSize = config.TileSize;
                }
                catch { } // Ignore any validation error
            }
            NotifyOfPropertyChange(nameof(AutomaticWorldName));
            NotifyOfPropertyChange(nameof(AutomaticPboPrefix));
            NotifyOfPropertyChange(nameof(AutomaticTargetModDirectory));
            NotifyOfPropertyChange(nameof(AutomaticTileSize));
        }

        public override string SouthWest
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
                NotifyCoordinatesRelated();
                IsDirty = true;
            }
        }

        public override float GridCellSize
        {
            get { return Config.GridCellSize; }
            set
            {
                Config.GridCellSize = GridHelper.NormalizeCellSize(value);
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridCellSize));
                NotifyCoordinatesRelated();
                IsDirty = true;
            }
        }

        public override int GridSize 
        { 
            get { return Config.GridSize; }
            set 
            {
                Config.GridSize = value;
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridSize));
                NotifyCoordinatesRelated();
                IsDirty = true;
            }
        }

        public override float MapSize 
        { 
            get { return Config.GridSize * Config.GridCellSize; }
            set
            {
                Config.GridSize = GridHelper.GetGridSize(GridSizes, value);
                Config.GridCellSize = GridHelper.NormalizeCellSize(value / Config.GridSize);
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridSize));
                NotifyOfPropertyChange(nameof(GridCellSize));
                NotifyCoordinatesRelated();
                IsDirty = true;
            }
        }

        public string WorldName
        {
            get { return Config.WorldName ?? string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Arma3ConfigHelper.ValidateWorldName(value);
                }
                Config.WorldName = value;
                NotifyOfPropertyChange(nameof(WorldName));
                UpdateAutomaticValues();
                IsDirty = true;
            }
        }

        public string PboPrefix
        {
            get { return Config.PboPrefix ?? string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    Arma3ConfigHelper.ValidatePboPrefix(value);
                }
                Config.PboPrefix = value;
                NotifyOfPropertyChange(nameof(PboPrefix));
                IsDirty = true;
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
                        MissingMods = await MissingMod.DetectMissingMods(_arma3DataModule, IoC.Get<IArma3ModsService>(), IoC.Get<ISubstituteDataService>(), deps);
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

        public string AutomaticWorldName { get; private set; }
        public string AutomaticPboPrefix { get; private set; }
        public string AutomaticTargetModDirectory { get; private set; }
        public int AutomaticTileSize { get; private set; } = 512;

        protected override async Task DoLoad(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            Config = await JsonSerializer.DeserializeAsync<Arma3MapConfigJson>(stream) ?? new Arma3MapConfigJson();
            Config.PrivateServiceRoadThreshold = Config.PrivateServiceRoadThreshold ?? MapProcessingOptions.Default.PrivateServiceRoadThreshold;
            Config.Satellite = Config.Satellite ?? new SatelliteImageOptions();
            NotifyOfPropertyChange(nameof(Config));
            NotifyOfPropertyChange(nameof(SouthWest));
            NotifyOfPropertyChange(nameof(Center));
            NotifyOfPropertyChange(nameof(MapSize));
            NotifyOfPropertyChange(nameof(GridSize));
            NotifyOfPropertyChange(nameof(GridCellSize));
            NotifyOfPropertyChange(nameof(BoundingBox));
            NotifyOfPropertyChange(nameof(AssetConfigFile));
            NotifyOfPropertyChange(nameof(UseColorCorrection));
            NotifyOfPropertyChange(nameof(UseRawColors));
            NotifyCoordinatesRelated();
            await CheckDependencies(); 
            await IoC.Get<IRecentFilesService>().AddRecentFile(filePath);
        }

        protected override async Task DoNew()
        {
            Config = new Arma3MapConfigJson();
            var assetConfig = _shell.Documents.OfType<AssetConfigEditorViewModel>().Select(r => r.FilePath ?? r.FileName).FirstOrDefault();
            Config.AssetConfigFile = assetConfig ?? BuiltinAssetConfigFiles.FirstOrDefault() ?? string.Empty;
            Config.Satellite = Config.Satellite ?? new SatelliteImageOptions();
            NotifyOfPropertyChange(nameof(Config));
            NotifyOfPropertyChange(nameof(AssetConfigFile));
            await CheckDependencies();
        }

        protected override async Task DoSave(string filePath)
        {
            using var stream = File.Create(filePath);
            await SaveTo(stream);
            await IoC.Get<IRecentFilesService>().AddRecentFile(filePath);
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
            var config = await GetBuildersConfigSafe(a3config);
            var render = new PreviewRender(a3config.TerrainArea, a3config.Imagery, config, IoC.Get<IGrmConfigService>().GetSources());
            var target = Path.Combine(Path.GetTempPath(), "grm-preview.html");
            await render.RenderHtml(task.Scope, target, ignoreElevation);
            task.AddSuccessAction(() => ShellHelper.OpenUri(target), Labels.ViewResultInWebBrowser);
        }


        internal async Task<IBuildersConfig> GetBuildersConfigSafe(Arma3MapConfig a3config)
        {
            try
            {
                return await GetAssets(_arma3DataModule.Library, a3config);
            }
            catch
            {
                return new DefaultBuildersConfig();
            }
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
                .RunTask(Labels.GenerateMapForArma3WRP, DoGenerateMap);
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

            var generator = new Arma3MapGenerator(assets, _arma3DataModule.ProjectDrive, _arma3DataModule.CreatePboCompilerFactory(), IoC.Get<IGrmConfigService>().GetSources());

            var freindlyName = await generator.GenerateMod(task.Scope, a3config);

            if (!string.IsNullOrEmpty(freindlyName))
            {
                var name = freindlyName;
                if (name != a3config.WorldName)
                {
                    name = $"{name} - {a3config.WorldName}"; // use WorldName to make it unique
                }
                task.AddSuccessAction(() => ShellHelper.OpenUri(a3config.TargetModDirectory), Labels.ViewInFileExplorer);
                task.AddSuccessAction(() => ShellHelper.OpenUri("steam://run/107410"), Labels.OpenArma3Launcher, string.Format(Labels.OpenArma3LauncherWithGeneratedModHint, name));
                task.AddSuccessAction(() => Arma3Helper.Launch(assets.Dependencies, a3config.TargetModDirectory, a3config.WorldName), Labels.LaunchArma3, Labels.LaunchArma3Hint);
                await Arma3LauncherHelper.CreateLauncherPresetAsync(assets.Dependencies, a3config.TargetModDirectory, "GRM - " + name);

                await AddToHistory(a3config, freindlyName);
            }
        }

        private async Task DoGenerateMap(IProgressTaskUI task)
        {
            var a3config = Config.ToArma3MapConfig();

            ReportConfig(task, _arma3DataModule.ProjectDrive, a3config);

            var assets = await GetAssets(_arma3DataModule.Library, a3config);

            var generator = new Arma3MapGenerator(assets, _arma3DataModule.ProjectDrive, _arma3DataModule.CreatePboCompilerFactory(), IoC.Get<IGrmConfigService>().GetSources());

            var results = await generator.GenerateWrp(task.Scope, a3config);

            task.AddSuccessAction(() => ShellHelper.OpenUri(_arma3DataModule.ProjectDrive.GetFullPath(a3config.PboPrefix)), Labels.ViewInFileExplorer);

            if (results != null)
            {
                await AddToHistory(a3config, results.FreindlyName);
            }
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
                .RunTask(Labels.GenerateSatelliteImage, (t) => DoImagery(t, async s => await (await s.CreateSatMap()).OffloadAsync()));
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
                .RunTask(Labels.GenerateIdMapImage, (t) => DoImagery(t, async s => await (await s.CreateIdMap()).OffloadAsync()));
            return Task.CompletedTask;
        }

        private async Task DoImagery(IProgressTaskUI task, Func<IImagerySource,Task> action)
        {
            var projectDrive = _arma3DataModule.ProjectDrive;
            var library = _arma3DataModule.Library;

            var a3config = Config.ToArma3MapConfig();

            Arma3Assets assets = await GetAssets(library, a3config);

            var generator = new Arma3MapGenerator(assets, projectDrive, _arma3DataModule.CreatePboCompilerFactory(), IoC.Get<IGrmConfigService>().GetSources());

            var target = Path.Combine(Path.GetTempPath(), a3config.WorldName);

            Directory.CreateDirectory(target);

            var source = await generator.GetImagerySource(task.Scope, a3config, new PersistentHugeImageStorage(target));
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

            var generator = new Arma3MapGenerator(assets, projectDrive, _arma3DataModule.CreatePboCompilerFactory(), IoC.Get<IGrmConfigService>().GetSources());

            var target = Path.Combine(Path.GetTempPath(), a3config.WorldName);
            Directory.CreateDirectory(target);

            var source = await generator.GetBuildContext(task.Scope, a3config, new PersistentHugeImageStorage(target));

            if (source != null)
            {
                await (await source.GetDataAsync<RawSatelliteImageData>()).Image.OffloadAsync();
            }
            task.AddSuccessAction(() => ShellHelper.OpenUri(target), Labels.ViewInFileExplorer);
        }

        private void ReportConfig(IProgressTaskUI task, ProjectDrive projectDrive, Arma3MapConfig a3config)
        {
            task.Scope.WriteLine($"MountPath='{projectDrive.MountPath}'");
            if (projectDrive.SecondarySource is PboFileSystem pbo)
            {
                foreach(var path in pbo.GamePaths)
                {
                    task.Scope.WriteLine($"GamePaths+='{path}'");
                }
                foreach (var path in pbo.ModsPaths)
                {
                    task.Scope.WriteLine($"ModsPaths+='{path}'");
                }
            }
        }

        public async Task SaveTo(Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, Config).ConfigureAwait(false);
        }

        public Task GenerateTerrainBuilder()
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateMapForArma3TB, DoGenerateTerrainBuilder);
            return Task.CompletedTask;
        }

        private async Task DoGenerateTerrainBuilder(IProgressTaskUI task)
        {
            var a3config = Config.ToArma3MapConfig();

            var target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameRealisticMap", "Arma3", "TerrainBuilder", a3config.WorldName);

            Directory.CreateDirectory(target);

            ReportConfig(task, _arma3DataModule.ProjectDrive, a3config);

            var assets = await GetAssets(_arma3DataModule.Library, a3config);

            var generator = new Arma3TerrainBuilderGenerator(assets, _arma3DataModule.ProjectDrive, IoC.Get<IGrmConfigService>().GetSources());

            var freindlyName = await generator.GenerateTerrainBuilderFiles(task.Scope, a3config, target);

            task.AddSuccessAction(() => ShellHelper.OpenUri(Path.Combine(target, "README.txt")), GameRealisticMap.Studio.Labels.ViewImportInstructions);
            task.AddSuccessAction(() => ShellHelper.OpenUri(target), Labels.ViewInFileExplorer);
            task.AddSuccessAction(() =>
            {
                Arma3ToolsHelper.EnsureProjectDrive();
                ShellHelper.OpenUri(Path.Combine(Arma3ToolsHelper.GetArma3ToolsPath(), "TerrainBuilder\\TerrainBuilder.exe"));
            }, GameRealisticMap.Studio.Labels.LaunchTerrainBuilder);

            if (!string.IsNullOrEmpty(freindlyName))
            {
                await AddToHistory(a3config, freindlyName);
            }
        }

        private async Task AddToHistory(Arma3MapConfig a3config, string? freindlyName)
        {
            await IoC.Get<IArma3RecentHistory>().RegisterWorld(
                a3config.WorldName,
                a3config.PboPrefix,
                freindlyName + ", GameRealisticMap",
                a3config.TargetModDirectory,
                IsNew ? null : FilePath);
        }

        internal override async Task<(IBuildersConfig, IMapProcessingOptions, ITerrainArea)> GetPreviewConfig()
        {
            var a3config = Config.ToArma3MapConfig();
            var config = await GetBuildersConfigSafe(a3config);
            return (config, a3config, a3config.TerrainArea);
        }

        public bool UseColorCorrection
        {
            get { return Config.UseColorCorrection; }
            set
            {
                if (Config.UseColorCorrection != value)
                {
                    Config.UseColorCorrection = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(nameof(UseRawColors));
                }
            }
        }
        public bool UseRawColors
        {
            get { return !UseColorCorrection; }
            set { UseColorCorrection = !value; }
        }

        public bool ShowPreview
        {
            get { return _showPreview; }
            set
            {
                if (_showPreview != value)
                {
                    _showPreview = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public ImageSource SatelliteImage1 { get; private set; }
        public ImageSource SatelliteImage2 { get; private set; }
        public ImageSource SatelliteImage3 { get; private set; }
        public string SatelliteImageProviderName { get; private set; }
        public async Task TestSatelliteColor()
        {
            var area = MapSelection?.TerrainArea;
            if (area == null)
            {
                return;
            }
            var sources = IoC.Get< IGrmConfigService>().GetSources();
            using var satProvider = new SatelliteImageProvider(new Pmad.ProgressTracking.NoProgress(), sources);

            var center = area.TerrainPointToLatLng(new Geometries.TerrainPoint(area.SizeInMeters/2, area.SizeInMeters / 2));
            using var img0 = await satProvider.GetTile(center);
            using var img1 = img0.CloneAs<Bgra32>();
            img1.Mutate(d => d.Resize(256, 256));


            using var img2 = img1.Clone();
            img2.Mutate(d => d.Brightness(Config.Satellite!.Brightness).Contrast(Config.Satellite!.Contrast).Saturate(Config.Satellite!.Saturation));
            
            using var img3 = img2.Clone();
            if (Config.UseColorCorrection)
            {
                Arma3ColorRender.Mutate(img3, Arma3ColorRender.FromArma3);
            }

            SatelliteImage1 = img1.ToWpf();
            SatelliteImage2 = img2.ToWpf();
            SatelliteImage3 = img3.ToWpf();
            SatelliteImageProviderName = SatelliteImageProvider.GetName(sources);

            NotifyOfPropertyChange(nameof(SatelliteImage1));
            NotifyOfPropertyChange(nameof(SatelliteImage2));
            NotifyOfPropertyChange(nameof(SatelliteImage3));
            NotifyOfPropertyChange(nameof(SatelliteImageProviderName));

            ShowPreview = true;
        }
    }
}
