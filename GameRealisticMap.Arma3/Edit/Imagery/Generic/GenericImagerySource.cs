using BIS.PAA;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages;
using Pmad.HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    public class GenericImagerySource : IImagerySource
    {
        private readonly GenericImageryInfos imagery;
        private readonly IGameFileSystem projectDrive;
        private readonly TerrainMaterialLibrary materials;

        public GenericImagerySource(GenericImageryInfos imagery, IGameFileSystem projectDrive, TerrainMaterialLibrary materials)
        {
            this.imagery = imagery;
            this.projectDrive = projectDrive;
            this.materials = materials;
        }

        public Task<HugeImage<Rgba32>> CreateIdMap()
        {
            return Task.FromResult(imagery.GetIdMap<Rgba32>(projectDrive, materials));
        }

        public async Task<Image> CreatePictureMap()
        {
            var existing = LoadExisting("picturemap_ca");
            if (existing != null)
            {
                return existing;
            }
            using var satMap = imagery.GetSatMap(projectDrive);
            return await satMap.ToScaledImageAsync(2048, 2048);
        }

        public Task<HugeImage<Rgba32>> CreateSatMap()
        {
            return Task.FromResult(imagery.GetSatMap<Rgba32>(projectDrive));
        }

        public Image CreateSatOut()
        {
            var existing = LoadExisting("satout_ca");
            if (existing != null)
            {
                return existing;
            }
            return new Image<Rgb24>(4, 4);
        }

        private Image? LoadExisting(string name)
        {
            using var paaStream = projectDrive.OpenFileIfExists($"{imagery.PboPrefix}\\data\\{name}.paa");
            if (paaStream != null)
            {
                var paa = new PAA(paaStream);
                var map = paa.Mipmaps.OrderBy(m => m.Width).Last();
                var pixels = PAA.GetARGB32PixelData(paa, paaStream, map);
                return Image.LoadPixelData<Bgra32>(pixels, map.Width, map.Height).CloneAs<Rgb24>();
            }
            using var png = projectDrive.OpenFileIfExists($"{imagery.PboPrefix}\\data\\{name}.png");
            if (png != null)
            {
                return Image.Load(png);
            }
            return null;
        }
    }
}