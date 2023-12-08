using System.Windows.Media.Media3D;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3Preview3D
    {
        Model3DGroup? GetModel(string path);
    }
}
