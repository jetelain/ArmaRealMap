using System.Collections.Generic;
using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    /// <summary>
    /// Resolves which Arma 3 mods are required by a set of P3D file paths.
    /// Used to populate the <c>requiredAddons</c> list when generating a terrain PBO config.
    /// </summary>
    internal interface IArma3Dependencies
    {
        IEnumerable<ModDependencyDefinition> ComputeModDependencies(IEnumerable<string> usedFiles);
    }
}