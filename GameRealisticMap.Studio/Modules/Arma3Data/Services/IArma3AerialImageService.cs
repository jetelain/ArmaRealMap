using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using GameRealisticMap.Arma3.Assets;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3AerialImageService
    {
        Uri? GetImageUri(string model);

        BitmapSource? GetImage(string model);

        Task TakeImages(IEnumerable<string> models, IEnumerable<ModDependencyDefinition> mods, IProgressScope progressSystem, bool onlyMissing = true);

        int CountMissing(IEnumerable<string> models);
    }
}
