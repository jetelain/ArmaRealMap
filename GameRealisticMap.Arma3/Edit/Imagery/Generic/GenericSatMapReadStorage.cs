using BIS.PAA;
using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    internal sealed class GenericSatMapReadStorage : IHugeImageStorageSlot
    {
        private readonly GenericImageryInfos partitioner;
        private readonly IGameFileSystem fileSystem;
        private readonly int tileSize;

        public GenericSatMapReadStorage(GenericImageryInfos partitioner, IGameFileSystem fileSystem)
        {
            this.partitioner = partitioner;
            this.fileSystem = fileSystem;
            tileSize = partitioner.TileSize;
        }

        public void Dispose()
        {

        }

        private string GetPartFileName(int partId)
        {
            return partitioner.OtherTileInfos[partId - 1].Sat;
        }

        public Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var fileName = GetPartFileName(partId);
            using var paaStream = fileSystem.OpenFileIfExists(fileName);
            if (paaStream == null)
            {
                throw new FileNotFoundException($"File '{fileName}' was not found.");
            }
            var paa = new PAA(paaStream);
            var map = paa.Mipmaps.OrderBy(m => m.Width).Last();
            var pixels = PAA.GetARGB32PixelData(paa, paaStream, map);
            var img = Image.LoadPixelData<Bgra32>(pixels, map.Width, map.Height).CloneAs<TPixel>();
            if (img.Width != tileSize || img.Height != tileSize)
            {
                img.Mutate(img => img.Resize(tileSize, tileSize));
            }
            return Task.FromResult(img)!;
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            throw new NotSupportedException();
        }


    }
}
