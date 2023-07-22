using System.Runtime.Versioning;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IPboCompilerFactory
    {
        [SupportedOSPlatform("windows")]
        IPboCompiler Create(IProgressTask task);
    }
}
