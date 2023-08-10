using System;
using System.IO;
using System.Threading.Tasks;
using BIS.Core.Streams;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class Arma3WorldEditorViewModel : PersistedDocument
    {
        private EditableWrp? _world;
        private ConfigFileData? _configFile;
        private string _targetModDirectory = string.Empty;
        private readonly IArma3DataModule arma3Data;
        private readonly IWindowManager windowManager;
        private readonly IArma3RecentHistory history;

        public Arma3WorldEditorViewModel(IArma3DataModule arma3Data, IWindowManager windowManager, IArma3RecentHistory history) 
        {
            this.arma3Data = arma3Data;
            this.windowManager = windowManager;
            this.history = history;
            OpenConfigFileCommand = new AsyncCommand(OpenConfigFile);
        }

        public IAsyncCommand OpenConfigFileCommand { get; }

        public bool IsLoading { get; set; }

        protected override async Task DoLoad(string filePath)
        {
            IsLoading = true;
            NotifyOfPropertyChange(nameof(IsLoading));

            World = StreamHelper.Read<AnyWrp>(filePath).GetEditableWrp();

            var worldName = Path.GetFileNameWithoutExtension(filePath);
            var configFile = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, "config.cpp");
            if (File.Exists(configFile))
            {
                ConfigFile = ConfigFileData.ReadFromFile(configFile, worldName);
            }
            else
            {
                ConfigFile = null;
            }

            HistoryEntry = await history.GetEntryOrDefault(worldName);

            TargetModDirectory = HistoryEntry?.ModDirectory ?? Arma3MapConfig.GetAutomaticTargetModDirectory(worldName);

            IsLoading = false;
            NotifyOfPropertyChange(nameof(IsLoading));
            NotifyOfPropertyChange(nameof(HistoryEntry));
        }

        public EditableWrp? World
        {
            get { return _world; }
            set { _world = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Size)); }
        }

        public ConfigFileData? ConfigFile
        {
            get { return _configFile; }
            set { _configFile = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(CanGenerateMod)); }
        }

        public string TargetModDirectory
        {
            get { return _targetModDirectory; }
            set { _targetModDirectory = value; NotifyOfPropertyChange(); }
        }

        public IArma3RecentEntry? HistoryEntry { get; private set; }

        public bool CanGenerateMod => !string.IsNullOrEmpty(_configFile?.PboPrefix);

        public float? CellSize
        {
            get
            {
                if (_world != null)
                {
                    return (_world.LandRangeX * _world.CellSize) / _world?.TerrainRangeX;
                }
                return null;
            }
        }

        public float? SizeInMeters
        {
            get
            {
                if (_world != null)
                {
                    return _world.LandRangeX * _world.CellSize;
                }
                return null;
            }
        }

        public string Size => $"{_world?.TerrainRangeX} × {CellSize} m ➜ {SizeInMeters} m";

        public IGameFileSystem GameFileSystem => arma3Data.ProjectDrive;

        protected override Task DoNew()
        {
            throw new NotSupportedException("You cannot create an empty world file.");
        }

        protected override Task DoSave(string filePath)
        {
            if (World != null)
            {
                StreamHelper.Write(World, filePath);
                if (ConfigFile != null)
                {
                    ConfigFile.SaveToFile();
                }
            }
            return Task.CompletedTask;
        }

        public async Task GenerateMod()
        {
            if (ConfigFile == null || _world == null)
            {
                return;
            }

            if (IsDirty)
            {
                await Save();
            }

            var thoericFilepath = arma3Data.ProjectDrive.GetFullPath(ConfigFile.PboPrefix + "\\" + FileName);
            if (!PathUtility.IsSameFile(thoericFilepath, FilePath)) // works across subst and mklink
            {
                throw new ApplicationException($"File '{FilePath}' is not located in project drive, should be located in '{thoericFilepath}'.");
            }

            Arma3ToolsHelper.EnsureProjectDrive();

            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateModForArma3, DoGenerateMod);
        }

        private async Task DoGenerateMod(IProgressTaskUI task)
        {
            if (ConfigFile == null || _world == null)
            {
                return;
            }

            var wrpConfig = new SimpleWrpModConfig(ConfigFile.WorldName, ConfigFile.PboPrefix, TargetModDirectory);

            var generator = new SimpleWrpModGenerator(arma3Data.ProjectDrive, arma3Data.CreatePboCompilerFactory());

            await generator.GenerateMod(task, wrpConfig, _world);

            task.AddSuccessAction(() => ShellHelper.OpenUri(wrpConfig.TargetModDirectory), Labels.ViewInFileExplorer);
            //task.AddSuccessAction(() => ShellHelper.OpenUri("steam://run/107410"), Labels.OpenArma3Launcher, string.Format(Labels.OpenArma3LauncherWithGeneratedModHint, name));
            //task.AddSuccessAction(() => Arma3Helper.Launch(assets.Dependencies, a3config.TargetModDirectory, a3config.WorldName), Labels.LaunchArma3, Labels.LaunchArma3Hint);
            //await Arma3LauncherHelper.CreateLauncherPresetAsync(assets.Dependencies, a3config.TargetModDirectory, "GRM - " + name);

            await IoC.Get<IArma3RecentHistory>().RegisterWorld(
                wrpConfig.WorldName,
                wrpConfig.PboPrefix,
                ConfigFile.Description,
                wrpConfig.TargetModDirectory);
        }

        public Task ImportEden()
        {
            return windowManager.ShowDialogAsync(new EdenImporterViewModel(this));
        }

        internal void Apply(WrpEditBatch batch)
        {
            if (World == null)
            {
                return;
            }
            using var task = IoC.Get<IProgressTool>().StartTask("Import");
            var processor = new WrpEditProcessor(task);
            processor.Process(World, batch);
            if ( ConfigFile != null)
            {
                ConfigFile.Revision++;
            }
            IsDirty = true;
        }

        public async Task OpenConfigFile()
        {
            var file = HistoryEntry?.ConfigFile;
            if (!string.IsNullOrEmpty(file))
            {
                await EditorHelper.OpenWithEditor("Arma3MapConfigEditorProvider", file);
            }
        }
    }
}
