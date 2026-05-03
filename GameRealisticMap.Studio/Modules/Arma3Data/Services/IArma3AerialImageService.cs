using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GameRealisticMap.Arma3.Assets;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    /// <summary>
    /// Captures aerial (top-down) screenshot images of Arma 3 models using the
    /// Arma 3 aerial extension. Images are cached and used in the asset browser
    /// to show a representative top view of each 3D model.
    /// </summary>
    internal interface IArma3AerialImageService
    {
        Uri? GetImageUri(string model);

        BitmapSource? GetImage(string model);

        Task TakeImages(IEnumerable<string> models, IEnumerable<ModDependencyDefinition> mods, IProgressScope progressSystem, bool onlyMissing = true);

        int CountMissing(IEnumerable<string> models);
    }
}
