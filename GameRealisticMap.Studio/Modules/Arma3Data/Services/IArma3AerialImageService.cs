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
    /// Arma 3 aerial extension. Images are cached and used in the map editor
    /// to show a representative top view of placed 3D model.
    /// 
    /// Might be used in the future to generate satellite textures for the map, 
    /// but currently only used for map editor.
    /// </summary>
    internal interface IArma3AerialImageService
    {
        Uri? GetImageUri(string model);

        BitmapSource? GetImage(string model);

        Task TakeImages(IEnumerable<string> models, IEnumerable<ModDependencyDefinition> mods, IProgressScope progressSystem, bool onlyMissing = true);

        int CountMissing(IEnumerable<string> models);
    }
}
