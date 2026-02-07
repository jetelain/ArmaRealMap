using BIS.PAA;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    internal sealed class GenericIdMapReadStorage : IHugeImageStorageSlot
    {
        private readonly GenericImageryInfos partitioner;
        private readonly IGameFileSystem fileSystem;
        private readonly Dictionary<string, TerrainMaterial> materials;
        private readonly int tileSize;

        public GenericIdMapReadStorage(GenericImageryInfos partitioner, IGameFileSystem fileSystem, TerrainMaterialLibrary library)
        {
            this.partitioner = partitioner;
            this.fileSystem = fileSystem;
            materials = library.Definitions.Select(d => d.Material).ToDictionary(m => m.GetColorTexturePath(partitioner), m => m, StringComparer.OrdinalIgnoreCase);
            tileSize = partitioner.TileSize * partitioner.IdMapMultiplier;
        }

        public void Dispose()
        {

        }

        public Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var part = partitioner.OtherTileInfos[partId-1];

            var textures = part.Textures.Select(tex => materials[tex]).Select(t => t.Id).ToList();

            using var paaStream = fileSystem.OpenFileIfExists(part.Mask);
            if (paaStream == null)
            {
                throw new FileNotFoundException($"File '{part.Mask}' was not found.");
            }
            var paa = new PAA(paaStream);
            var map = paa.Mipmaps.OrderBy(m => m.Width).Last();
            var pixels = PAA.GetARGB32PixelData(paa, paaStream, map);
            var maskImage = Image.LoadPixelData<Bgra32>(pixels, map.Width, map.Height).CloneAs<Rgba32>();
            if (maskImage.Width != tileSize || maskImage.Height != tileSize)
            {
                maskImage.Mutate(img => img.Resize(tileSize, tileSize));
            }
            var finalImage = new Image<TPixel>(maskImage.Width, maskImage.Height);
            var px = new TPixel();
            for (int x = 0; x < maskImage.Width; ++x)
            {
                for (int y = 0; y < maskImage.Height; ++y)
                {
                    px.FromRgb24(IdMapReadStorage.GetColor(maskImage[x, y], textures));
                    finalImage[x, y] = px;
                }
            }
            return Task.FromResult(finalImage)!;
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            throw new NotSupportedException();
        }
    }
}
