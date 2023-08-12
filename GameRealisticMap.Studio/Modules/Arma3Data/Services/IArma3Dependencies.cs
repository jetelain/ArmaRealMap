using System.Collections.Generic;
using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal interface IArma3Dependencies
    {
        IEnumerable<ModDependencyDefinition> ComputeModDependencies(IEnumerable<string> usedFiles);
    }
}