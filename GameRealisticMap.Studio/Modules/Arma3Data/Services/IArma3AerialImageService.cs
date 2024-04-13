using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3AerialImageService
    {
        Uri? GetImageUri(string model);

        Task TakeImages(IEnumerable<string> models, IEnumerable<ModDependencyDefinition> mods, IProgressSystem progressSystem, bool onlyMissing = true);

        int CountMissing(IEnumerable<string> models);
    }
}
