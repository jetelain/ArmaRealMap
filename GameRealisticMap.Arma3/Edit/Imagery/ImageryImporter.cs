using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;
using HugeImages;
using HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public class ImageryImporter
    {
        private readonly ProjectDrive projectDrive;
        private readonly TerrainMaterialLibrary materials;
        private readonly IProgressSystem progress;

        public ImageryImporter(ProjectDrive projectDrive, IProgressSystem progress)
            : this(projectDrive, new TerrainMaterialLibrary(), progress)
        {

        }

        public ImageryImporter(ProjectDrive projectDrive, TerrainMaterialLibrary materials, IProgressSystem progress)
        {
            this.projectDrive = projectDrive;
            this.materials = materials;
            this.progress = progress;
        }

        public async Task UpdateIdMap(ExistingImageryInfos infos, string sourceFile)
        {
            using var himage = await LoadImage(sourceFile);

            EnsureImageSize(infos, himage);

            var compiled = new ImageryCompiler(materials, progress, projectDrive);

            compiled.GenerateIdMapTilesAndRvMat(infos, himage, infos.CreateTiler());

            await projectDrive.ProcessImageToPaa(progress);
        }

        private static void EnsureImageSize(ExistingImageryInfos infos, HugeImage<Rgba32> himage)
        {
            if (himage.Size.Width != infos.TotalSize || himage.Size.Height != infos.TotalSize)
            {
                throw new ApplicationException($"Image size is {himage.Size.Width}x{himage.Size.Height} but it must be {infos.TotalSize}x{infos.TotalSize} for this map");
            }
        }

        public async Task UpdateSatMap(ExistingImageryInfos infos, string sourceFile)
        {
            using var himage = await LoadImage(sourceFile);

            EnsureImageSize(infos, himage);

            var compiled = new ImageryCompiler(materials, progress, projectDrive);

            compiled.GenerateSatMapTiles(infos, himage, infos.CreateTiler());

            await projectDrive.ProcessImageToPaa(progress);
        }

        private async Task<HugeImage<Rgba32>> LoadImage(string sourceFile)
        {
            using (var report = progress.CreateStep("LoadImage",1))
            {
                return await StorageExtensions.LoadUniqueAsync<Rgba32>(sourceFile);
            }
        }
    }
}
