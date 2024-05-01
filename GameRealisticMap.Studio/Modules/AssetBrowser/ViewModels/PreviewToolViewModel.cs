using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using Gemini.Framework;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export]
    internal class PreviewToolViewModel : Tool
    {
        public override PaneLocation PreferredLocation => PaneLocation.Right;

        private readonly IArma3Preview3D _arma3Preview3D;

        [ImportingConstructor]
        public PreviewToolViewModel(IArma3Preview3D arma3Preview3D)
        {
            _arma3Preview3D = arma3Preview3D;
            DisplayName = Labels.Preview3D;
        }

        public void SetP3d(string path)
        {
            Model3DGroup = _arma3Preview3D.GetModel(path);
        }

        private Model3DGroup? _model3DGroup;
        public Model3DGroup? Model3DGroup { get => _model3DGroup; set { Set(ref _model3DGroup, value); } }

        private double _contrast = 0;
        public double Contrast { get => _contrast; set { Set(ref _contrast, value); } }

        private double _brightness = 0;
        public double Brightness { get => _brightness; set { Set(ref _brightness, value); } }

        public Task Reset()
        {
            Brightness = 0;
            Contrast = 0;
            return Task.CompletedTask;
        }

    }
}
