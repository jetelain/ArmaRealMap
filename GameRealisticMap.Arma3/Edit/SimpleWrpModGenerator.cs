using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Edit
{
    [SupportedOSPlatform("windows")]
    public class SimpleWrpModGenerator
    {
        private readonly ProjectDrive projectDrive;
        private readonly IPboCompilerFactory pboCompilerFactory;

        public SimpleWrpModGenerator(ProjectDrive projectDrive, IPboCompilerFactory pboCompilerFactory)
        {
            this.projectDrive = projectDrive;
            this.pboCompilerFactory = pboCompilerFactory;
        }

        public async Task GenerateMod(IProgressScope progress, SimpleWrpModConfig config, EditableWrp wrpContent)
        {
            var usedModels = wrpContent.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var usedRvmat = wrpContent.MatNames.Where(m => !string.IsNullOrEmpty(m)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            UnpackModels(progress, usedModels);

            await CreatePbo(progress, config, usedModels, usedRvmat);
        }

        private async Task CreatePbo(IProgressScope progress, SimpleWrpModConfig config, List<string> usedModels, List<string> usedRvmat)
        {
            Directory.CreateDirectory(config.TargetModDirectory);
            await pboCompilerFactory.Create(progress).BinarizeAndCreatePbo(config, usedModels, usedRvmat);
        }

        private void UnpackModels(IProgressScope progress, List<string> usedModels)
        {
            using (var report = progress.CreateInteger("UnpackModels", usedModels.Count))
            {
                foreach (var model in usedModels)
                {
                    if (!projectDrive.EnsureLocalFileCopy(model))
                    {
                        throw new ApplicationException($"File '{model}' is missing. Have you added all required mods in application configuration?");
                    }
                    report.ReportOneDone();
                }
            }
        }
    }
}
