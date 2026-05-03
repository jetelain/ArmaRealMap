using System.Windows.Media.Media3D;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    /// <summary>
    /// Loads Arma 3 P3D models as WPF <see cref="System.Windows.Media.Media3D.Model3DGroup"/> objects
    /// for display in the Studio 3D preview panel.
    /// </summary>
    internal interface IArma3Preview3D
    {
        Model3DGroup? GetModel(string path);
    }
}
