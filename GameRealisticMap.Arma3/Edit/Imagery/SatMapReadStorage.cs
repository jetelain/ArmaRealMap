using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    internal sealed class SatMapReadStorage : IHugeImageStorageSlot
    {
        private readonly IImageryPartitioner partitioner;
        private readonly IGameFileSystem fileSystem;
        private readonly string path;

        public SatMapReadStorage(IImageryPartitioner partitioner, IGameFileSystem fileSystem, string path)
        {
            this.partitioner = partitioner;
            this.fileSystem = fileSystem;
            this.path = path;
        }

        public void Dispose()
        {

        }

        private string GetPartFileName(int partId)
        {
            var part = partitioner.GetPartFromId(partId);
            var fileName = $"{path}\\data\\layers\\S_{part.X:000}_{part.Y:000}_lco.png";
            return fileName;
        }

        public async Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var fileName = GetPartFileName(partId);
            using var stream = fileSystem.OpenFileIfExists(fileName);
            if (stream == null)
            {
                throw new FileNotFoundException($"File '{fileName}' was not found.");
            }
            return await Image.LoadAsync<TPixel>(stream);
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            throw new NotSupportedException();
        }


    }
}
