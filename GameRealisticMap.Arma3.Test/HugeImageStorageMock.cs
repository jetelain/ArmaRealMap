using System.Collections.Concurrent;
using HugeImages;
using HugeImages.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Test
{
    internal class HugeImageStorageMock : IHugeImageStorageSlot, IHugeImageStorage
    {
        internal ConcurrentDictionary<int, Image> Storage { get; } = new ConcurrentDictionary<int, Image>();

        public IHugeImageStorageSlot CreateSlot(string name, HugeImageSettingsBase settings)
        {
            return this;
        }

        public void Dispose()
        {
            Storage.Clear();
        }

        public Task<Image<TPixel>?> LoadImagePart<TPixel>(int partId) where TPixel : unmanaged, IPixel<TPixel>
        {
            if (Storage.TryGetValue(partId, out var image))
            {
                return Task.FromResult<Image<TPixel>?>(image.CloneAs<TPixel>());
            }
            return Task.FromResult<Image<TPixel>?>(null);
        }

        public Task SaveImagePart<TPixel>(int partId, Image<TPixel> partImage) where TPixel : unmanaged, IPixel<TPixel>
        {
            var clone = partImage.Clone();
            Storage[partId] = clone;
            return Task.CompletedTask;
        }
    }
}
