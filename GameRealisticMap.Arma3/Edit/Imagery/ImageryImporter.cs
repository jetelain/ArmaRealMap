using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;
using HugeImages;
using HugeImages.Storage;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public class ImageryImporter
    {
        private readonly ProjectDrive projectDrive;
        private readonly TerrainMaterialLibrary materials;
        private readonly IProgressScope progress;

        public ImageryImporter(ProjectDrive projectDrive, IProgressScope progress)
            : this(projectDrive, new TerrainMaterialLibrary(), progress)
        {

        }

        public ImageryImporter(ProjectDrive projectDrive, TerrainMaterialLibrary materials, IProgressScope progress)
        {
            this.projectDrive = projectDrive;
            this.materials = materials;
            this.progress = progress;
        }

        public async Task UpdateIdMap(ExistingImageryInfos infos, string sourceFile)
        {
            using var himage = await LoadImage(sourceFile);

            var actualMultiplier = EnsureIdMapSize(infos, himage);

            var compiled = new ImageryCompiler(materials, progress, projectDrive);

            infos.IdMapMultiplier = actualMultiplier;

            var usedMaterials = compiled.GenerateIdMapTilesAndRvMat(infos, himage, infos.CreateTiler());

            var subsetMaterials = new TerrainMaterialLibrary(materials.Definitions.Where(d => usedMaterials.Contains(d.Material)).ToList(), materials.TextureSizeInMeters);

            MaterialConfigGenerator.GenerateConfigFiles(projectDrive, infos, subsetMaterials);

            TerrainMaterialHelper.UnpackEmbeddedFiles(subsetMaterials, progress, projectDrive, infos);

            await projectDrive.ProcessImageToPaa(progress);
        }

        private static int EnsureIdMapSize(ExistingImageryInfos infos, HugeImage<Rgba32> himage)
        {
            foreach(var multiplier in Arma3MapConfig.ValidIdMapMultipliers)
            {
                if (himage.Size.Width == infos.TotalSize * multiplier && himage.Size.Height == infos.TotalSize * multiplier)
                {
                    return multiplier;
                }
            }
            throw new ApplicationException($"Image size is {himage.Size.Width}x{himage.Size.Height} but it must be {infos.TotalSize}x{infos.TotalSize} for this map (or x2, or x4)");
        }

        private static void EnsureSatMapSize(ExistingImageryInfos infos, HugeImage<Rgba32> himage)
        {
            if (himage.Size.Width != infos.TotalSize || himage.Size.Height != infos.TotalSize)
            {
                throw new ApplicationException($"Image size is {himage.Size.Width}x{himage.Size.Height} but it must be {infos.TotalSize}x{infos.TotalSize} for this map");
            }
        }

        public async Task UpdateSatMap(ExistingImageryInfos infos, string sourceFile)
        {
            using var himage = await LoadImage(sourceFile);

            EnsureSatMapSize(infos, himage);

            var compiled = new ImageryCompiler(materials, progress, projectDrive);

            compiled.GenerateSatMapTiles(infos, himage, infos.CreateTiler());

            await projectDrive.ProcessImageToPaa(progress);
        }

        private async Task<HugeImage<Rgba32>> LoadImage(string sourceFile)
        {
            using (var report = progress.CreateSingle("LoadImage"))
            {
                return await StorageExtensions.LoadUniqueAsync<Rgba32>(sourceFile);
            }
        }
    }
}
