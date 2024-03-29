using System.ComponentModel.Composition;
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
    }
}
