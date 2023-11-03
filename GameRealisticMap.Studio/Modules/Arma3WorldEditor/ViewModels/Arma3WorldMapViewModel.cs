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

        public Arma3WorldMapViewModel(Arma3WorldEditorViewModel parent, IArma3DataModule arma3Data)
        {
            this.parentEditor = parent;
            this.arma3Data = arma3Data;
        }

        public EditableArma3Roads? Roads { get; set; }

        public float SizeInMeters => parentEditor.SizeInMeters ?? 2500;

        public HugeImage<Rgb24>? SatMap { get; set; }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (parentEditor.ConfigFile != null && !string.IsNullOrEmpty(parentEditor.ConfigFile.Roads))
            {
                Roads = new RoadsDeserializer(arma3Data.ProjectDrive).Deserialize(parentEditor.ConfigFile.Roads);
                NotifyOfPropertyChange(nameof(Roads));
            }

            if ( parentEditor.Imagery != null)
            {
                SatMap = parentEditor.Imagery.GetSatMap(arma3Data.ProjectDrive);
            }

            return base.OnActivateAsync(cancellationToken);
        }
    }
}
