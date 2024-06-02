using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Generic;
using GameRealisticMap.Generic.Profiles;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Modules.Main;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Modules.MapConfigEditor;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.GenericMapConfigEditor.ViewModels
{
    internal class GenericMapConfigEditorViewModel : MapConfigEditorBase, IExplorerRootTreeItem, IMainDocument
    {
        private readonly IShell shell;
        private readonly IGrmConfigService grmConfig;
        private string _center = string.Empty;
        private string _southWest = string.Empty;
        private float _gridCellSize = 5;
        private int _gridSize = 512;
        private string _exportProfileFile = ExportProfile.Default;
        private string _targetDirectory = string.Empty;

        public GenericMapConfigEditorViewModel(IShell shell, IGrmConfigService grmConfig)
        {
            this.shell = shell;
            this.grmConfig = grmConfig;
        }

        public string TreeName => DisplayName;

        public string Icon => GenericMapConfigEditorProvider.IconSource;

        public IEnumerable<IExplorerTreeItem> Children => Enumerable.Empty<IExplorerTreeItem>();

        public List<string> BuiltinExportProfiles { get; } = ExportProfile.GetBuiltinList();

        public async Task SaveTo(Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, ToConfig());
        }

        private GenericMapConfigJson ToConfig()
        {
            return new GenericMapConfigJson()
            {
                Center = _center,
                SouthWest = _southWest,
                GridCellSize = _gridCellSize,
                GridSize = _gridSize,
                ExportProfileFile = _exportProfileFile,
                TargetDirectory = _targetDirectory
            };
        }

        protected override async Task DoLoad(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var config = await JsonSerializer.DeserializeAsync<GenericMapConfigJson>(stream);
            if (config != null)
            {
                _center = config.Center ?? string.Empty;
                _southWest = config.SouthWest ?? string.Empty;
                _gridCellSize = config.GridCellSize;
                _gridSize = config.GridSize;
                _exportProfileFile = config.ExportProfileFile ?? ExportProfile.Default;
                _targetDirectory = config.TargetDirectory ?? string.Empty;

                NotifyOfPropertyChange(nameof(Center));
                NotifyOfPropertyChange(nameof(SouthWest));
                NotifyOfPropertyChange(nameof(GridCellSize));
                NotifyOfPropertyChange(nameof(GridSize));
                NotifyOfPropertyChange(nameof(ExportProfileFile));
                NotifyOfPropertyChange(nameof(TargetDirectory));
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(MapSelection));
                NotifyOfPropertyChange(nameof(AutomaticTargetDirectory)); 
                
                await IoC.Get<IRecentFilesService>().AddRecentFile(filePath);
            }
        }

        protected override Task DoNew()
        {
            return Task.CompletedTask;
        }

        protected override async Task DoSave(string filePath)
        {
            await IoC.Get<IRecentFilesService>().AddRecentFile(filePath);

            using var stream = File.Create(filePath);
            await SaveTo(stream);
        }


        public override int[] GridSizes => GridHelper.GenericGridSizes;

        public override string Center
        {
            get { return _center; }
            set
            {
                if (Set(ref _center, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        SouthWest = string.Empty;
                    }
                    NotifyOfPropertyChange(nameof(MapSelection));
                    NotifyOfPropertyChange(nameof(AutomaticTargetDirectory));
                    IsDirty = true;
                }
            }
        }


        public override string SouthWest
        {
            get { return _southWest; }
            set
            {
                if (Set(ref _southWest, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        Center = string.Empty;
                    }
                    NotifyOfPropertyChange(nameof(MapSelection));
                    NotifyOfPropertyChange(nameof(AutomaticTargetDirectory));
                    IsDirty = true;
                }
            }
        }

        public override float GridCellSize
        {
            get { return _gridCellSize; }
            set
            {
                if (Set(ref _gridCellSize, GridHelper.NormalizeCellSize(value)))
                {
                    NotifyOfPropertyChange(nameof(MapSize));
                    NotifyOfPropertyChange(nameof(MapSelection));
                    NotifyOfPropertyChange(nameof(AutomaticTargetDirectory));
                    IsDirty = true;
                }
            }
        }

        public override int GridSize
        {
            get { return _gridSize; }
            set
            {
                if (Set(ref _gridSize, value))
                {
                    NotifyOfPropertyChange(nameof(MapSize));
                    NotifyOfPropertyChange(nameof(MapSelection));
                    NotifyOfPropertyChange(nameof(AutomaticTargetDirectory));
                    IsDirty = true;
                }
            }
        }

        public override float MapSize
        {
            get { return GridSize * GridCellSize; }
            set
            {
                GridSize = GridHelper.GetGridSize(GridSizes, value);
                GridCellSize = GridHelper.NormalizeCellSize(value / GridSize);
            }
        }

        public string ExportProfileFile
        {
            get { return _exportProfileFile; }
            set { Set(ref _exportProfileFile, value); }
        }

        public string TargetDirectory
        {
            get { return _targetDirectory; }
            set { Set(ref _targetDirectory, value); }
        }

        public string AutomaticTargetDirectory
        {
            get
            {
                var area = MapSelection?.TerrainArea;
                if (area != null)
                {
                    return GenericMapConfig.GetAutomaticTargetDirectory(area);
                }
                return string.Empty;
            }
        }

        public Task DoFullExport()
        {
            ProgressToolHelper.Start(new GenericExportTask(ToConfig().ToMapConfig(), grmConfig.GetSources()));
            return Task.CompletedTask;
        }

    }
}
