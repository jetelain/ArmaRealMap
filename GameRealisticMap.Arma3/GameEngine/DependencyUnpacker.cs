using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal class DependencyUnpacker
    {
        private readonly IArma3RegionAssets assets;
        private readonly ProjectDrive projectDrive;

        public DependencyUnpacker(IArma3RegionAssets assets, ProjectDrive projectDrive)
        {
            this.assets = assets;
            this.projectDrive = projectDrive;
        }

        public void Unpack(IProgressTask progress, WrpCompiler wrpBuilder)
        {
            UnpackFiles(progress, GetRequiredFiles(wrpBuilder.UsedModels));
        }

        public void Unpack(IProgressTask progress, IEnumerable<string> usedModels)
        {
            UnpackFiles(progress, GetRequiredFiles(usedModels));
        }

        private List<string> GetRequiredFiles(IEnumerable<string> usedModels)
        {
            var materials = Enum.GetValues<TerrainMaterialUsage>().Select(u => assets.Materials.GetMaterialByUsage(u)).ToList();
            var roads = Enum.GetValues<RoadTypeId>().Select(u => assets.RoadTypeLibrary.GetInfo(u)).ToList();
            return usedModels
                .Concat(materials.Select(m => m.NormalTexture).Distinct())
                .Concat(materials.Select(m => m.ColorTexture).Distinct())
                .Concat(roads.Select(m => m.TextureEnd).Distinct())
                .Concat(roads.Select(m => m.Texture).Distinct())
                .Concat(roads.Select(m => m.Material).Distinct())
                .ToList();
        }

        private void UnpackFiles(IProgressTask progress, IReadOnlyCollection<string> files)
        {
            if (!OperatingSystem.IsWindows())
            {
                return; // Unsupported on Linux
            }
            using var report = progress.CreateStep("UnpackFiles", files.Count);
            foreach (var model in files)
            {
                if (!projectDrive.EnsureLocalFileCopy(model))
                {
                    throw new ApplicationException($"File '{model}' is missing. Have you added all required mods in application configuration?");
                }
                report.ReportOneDone();
            }
            progress.ReportOneDone();
        }
    }
}
