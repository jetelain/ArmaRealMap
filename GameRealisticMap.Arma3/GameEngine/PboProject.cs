using System.Runtime.Versioning;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.GameEngine
{
    [SupportedOSPlatform("windows")]
    internal class PboProject : IPboCompiler
    {
        private readonly IProgressSystem progress;

        public PboProject(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public async Task BinarizeAndCreatePbo(Arma3MapConfig config, IReadOnlyCollection<string> usedModels, IReadOnlyCollection<string> usedRvmat)
        {
            await Arma3ToolsHelper.BuildWithMikeroPboProject(config.PboPrefix, config.TargetModDirectory, progress);
        }
    }
}
