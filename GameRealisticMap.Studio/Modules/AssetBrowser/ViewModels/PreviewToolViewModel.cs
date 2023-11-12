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
            DisplayName = "3D Preview";
        }

        public void SetP3d(string path)
        {
            Model3DGroup = _arma3Preview3D.GetModel(path);
            NotifyOfPropertyChange(nameof(Model3DGroup));
        }

        public Model3DGroup? Model3DGroup {  get; set; }
    }
}
