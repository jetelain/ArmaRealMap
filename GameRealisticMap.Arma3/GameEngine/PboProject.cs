using System.Runtime.Versioning;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.GameEngine
{
    [SupportedOSPlatform("windows")]
    internal class PboProject : IPboCompiler
    {
        private readonly IProgressScope progress;

        public PboProject(IProgressScope progress)
        {
            this.progress = progress;
        }

        public async Task BinarizeAndCreatePbo(IPboConfig config, IReadOnlyCollection<string> usedModels, IReadOnlyCollection<string> usedRvmat)
        {
            await Arma3ToolsHelper.BuildWithMikeroPboProject(config.PboPrefix, config.TargetModDirectory, progress);
        }
    }
}
