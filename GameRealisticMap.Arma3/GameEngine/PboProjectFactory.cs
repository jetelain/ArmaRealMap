using System.Runtime.Versioning;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{
    public sealed class PboProjectFactory : IPboCompilerFactory
    {
        [SupportedOSPlatform("windows")]
        public IPboCompiler Create(IProgressScope task)
        {
            return new PboProject(task);
        }
    }
}
