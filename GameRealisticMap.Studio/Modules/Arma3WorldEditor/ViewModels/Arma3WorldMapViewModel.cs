using System.Threading;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Gemini.Framework;
using HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class Arma3WorldMapViewModel : Document
    {
        private readonly Arma3WorldEditorViewModel parentEditor;
        private readonly IArma3DataModule arma3Data;

        public double BackgroundResolution { get; }

        private BackgroundMode _backgroundMode;

        public Arma3WorldMapViewModel(Arma3WorldEditorViewModel parent, IArma3DataModule arma3Data)
        {
            this.parentEditor = parent;
            this.arma3Data = arma3Data;
            BackgroundResolution = parentEditor.Imagery?.Resolution ?? 1;
            DisplayName = parent.DisplayName + " - Editor";
        }

        public EditableArma3Roads? Roads { get; set; }

        public float SizeInMeters => parentEditor.SizeInMeters ?? 2500;

        public HugeImage<Rgb24>? SatMap { get; set; }

        public HugeImage<Rgb24>? IdMap { get; set; }

        public HugeImage<Rgb24>? BackgroundImage
        {
            get
            {
                switch (BackgroundMode)
                {
                    case BackgroundMode.Satellite: return SatMap;
                    case BackgroundMode.TextureMask: return IdMap;
                    default: return null;
                }
            }
        }

        public BackgroundMode BackgroundMode
        {
            get { return _backgroundMode; }
            set
            {
                if (_backgroundMode != value)
                {
                    _backgroundMode = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(nameof(BackgroundImage));

                }


            }
        }

        protected async override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (parentEditor.ConfigFile != null && !string.IsNullOrEmpty(parentEditor.ConfigFile.Roads))
            {
                Roads = new RoadsDeserializer(arma3Data.ProjectDrive).Deserialize(parentEditor.ConfigFile.Roads);
                NotifyOfPropertyChange(nameof(Roads));
            }

            if (parentEditor.Imagery != null)
            {
                SatMap = parentEditor.Imagery.GetSatMap(arma3Data.ProjectDrive);

                var assets = await parentEditor.GetAssetsFromHistory();
                if (assets != null)
                {
                    IdMap = parentEditor.Imagery.GetIdMap(arma3Data.ProjectDrive, assets.Materials);
                }
            }

            await base.OnInitializeAsync(cancellationToken);
        }
    }
}
