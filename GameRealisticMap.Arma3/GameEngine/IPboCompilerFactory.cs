using System.Runtime.Versioning;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IPboCompilerFactory
    {
        [SupportedOSPlatform("windows")]
        IPboCompiler Create(IProgressScope task);
    }
}
