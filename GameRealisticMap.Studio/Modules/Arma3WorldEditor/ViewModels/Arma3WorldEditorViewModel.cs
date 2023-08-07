using System;
using System.IO;
using System.Threading.Tasks;
using BIS.Core.Streams;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Edit;
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
        private readonly IArma3DataModule arma3Data;
        private readonly IWindowManager windowManager;

        public Arma3WorldEditorViewModel(IArma3DataModule arma3Data, IWindowManager windowManager) 
        {
            this.arma3Data = arma3Data;
            this.windowManager = windowManager;
        }

        public bool IsLoading { get; set; }

        protected override Task DoLoad(string filePath)
        {
            IsLoading = true;
            NotifyOfPropertyChange(nameof(IsLoading));

            World = StreamHelper.Read<AnyWrp>(filePath).GetEditableWrp();

            var configFile = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, "config.cpp");
            if (File.Exists(configFile))
            {
                ConfigFile = ConfigFileData.ReadFromFile(configFile, Path.GetFileNameWithoutExtension(filePath));
                pendingRevision = ConfigFile.Revision;
            }
            else
            {
                ConfigFile = null;
            }

            IsLoading = false;
            NotifyOfPropertyChange(nameof(IsLoading));
            return Task.CompletedTask;
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

            var a3config = new SimpleWrpModConfig(ConfigFile.WorldName, ConfigFile.PboPrefix);

            var generator = new SimpleWrpModGenerator(arma3Data.ProjectDrive, arma3Data.CreatePboCompilerFactory());

            await generator.GenerateMod(task, a3config, _world);

            task.AddSuccessAction(() => ShellHelper.OpenUri(a3config.TargetModDirectory), Labels.ViewInFileExplorer);
            //task.AddSuccessAction(() => ShellHelper.OpenUri("steam://run/107410"), Labels.OpenArma3Launcher, string.Format(Labels.OpenArma3LauncherWithGeneratedModHint, name));
            //task.AddSuccessAction(() => Arma3Helper.Launch(assets.Dependencies, a3config.TargetModDirectory, a3config.WorldName), Labels.LaunchArma3, Labels.LaunchArma3Hint);
            //await Arma3LauncherHelper.CreateLauncherPresetAsync(assets.Dependencies, a3config.TargetModDirectory, "GRM - " + name);
        }

        public Task ImportEden()
        {
            // TODO !

            return Task.CompletedTask;
        }
    }
}
