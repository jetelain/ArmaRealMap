using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameRealisticMap.Arma3;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework;
using MapControl;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    internal class MapConfigEditorViewModel : PersistedDocument2, IExplorerRootTreeItem
    {
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
                Config.GridCellSize = value;
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
                Config.GridCellSize = value / Config.GridSize;
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridSize));
                NotifyOfPropertyChange(nameof(GridCellSize));
                NotifyOfPropertyChange(nameof(Locations));
                IsDirty = true;
            }
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
            NotifyOfPropertyChange(nameof(GridCellSize));
            NotifyOfPropertyChange(nameof(Locations));
        }

        protected override Task DoNew()
        {
            Config = new Arma3MapConfigJson();
            NotifyOfPropertyChange(nameof(Config));
            return Task.CompletedTask;
        }

        protected override async Task DoSave(string filePath)
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, Config);
        }

    }
}
