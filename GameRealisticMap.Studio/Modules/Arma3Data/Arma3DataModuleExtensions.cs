using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.Arma3Data
{
    internal static class Arma3DataModuleExtensions
    {
        public static IEnumerable<ModelInfo> Import(this IArma3DataModule module, IEnumerable<string> paths)
        {
            try
            {
                return paths.Select(module.ProjectDrive.GetGamePath).Select(p => module.Library.ResolveByPath(p)).ToList();
            }
            catch
            {
                return new List<ModelInfo>();
            }
        }
    }
}
