using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GameRealisticMap.Arma3;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels
{
    internal class MapConfigEditorViewModel : PersistedDocument
    {
        public Arma3MapConfigJson Config { get; set; } = new Arma3MapConfigJson();

        public int[] GridSizes { get; } = new int[] { 256, 512, 1024, 2048, 4096, 8192 };
        
        public float GridCellSize
        {
            get { return Config.GridCellSize; }
            set
            {
                Config.GridCellSize = value;
                NotifyOfPropertyChange(nameof(MapSize));
                NotifyOfPropertyChange(nameof(GridCellSize));
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
                IsDirty = true;
            }
        }



        protected override async Task DoLoad(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            Config = await JsonSerializer.DeserializeAsync<Arma3MapConfigJson>(stream) ?? new Arma3MapConfigJson();
            NotifyOfPropertyChange(nameof(Config));
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
