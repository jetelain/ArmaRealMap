﻿using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ManMade.Roads;
using Pmad.ProgressTracking;

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

        public void Unpack(IProgressScope progress, IArma3MapConfig config, WrpCompiler wrpBuilder)
        {
            UnpackFiles(progress, GetRequiredFiles(wrpBuilder.UsedModels, config));
        }

        public void Unpack(IProgressScope progress, IArma3MapConfig config, IEnumerable<string> usedModels)
        {
            UnpackFiles(progress, GetRequiredFiles(usedModels, config));
        }

        private List<string> GetRequiredFiles(IEnumerable<string> usedModels, IArma3MapConfig config)
        {
            var materials = assets.Materials.Definitions.Where(m => m.Data == null).Select(m => m.Material);
            var roads = Enum.GetValues<RoadTypeId>().Select(u => assets.RoadTypeLibrary.GetInfo(u)).ToList();
            return usedModels
                .Concat(materials.Select(m => m.GetNormalTexturePath(config)).Distinct())
                .Concat(materials.Select(m => m.GetColorTexturePath(config)).Distinct())
                .Concat(roads.Select(m => m.TextureEnd).Distinct())
                .Concat(roads.Select(m => m.Texture).Distinct())
                .Concat(roads.Select(m => m.Material).Distinct())
                .ToList();
        }

        private void UnpackFiles(IProgressScope progress, IReadOnlyCollection<string> files)
        {
            if (!OperatingSystem.IsWindows())
            {
                return; // Unsupported on Linux
            }
            using var report = progress.CreateInteger("UnpackFiles", files.Count);
            foreach (var model in files)
            {
                if (!projectDrive.EnsureLocalFileCopy(model) && !string.IsNullOrWhiteSpace(model))
                {
                    throw new ApplicationException($"File '{model}' is missing. Have you added all required mods in application configuration?");
                }
                report.ReportOneDone();
            }
        }
    }
}
