using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.GameEngine
{
    public sealed class PboProjectFactory : IPboCompilerFactory
    {
        [SupportedOSPlatform("windows")]
        public IPboCompiler Create(IProgressTask task)
        {
            return new PboProject(task);
        }
    }
}
