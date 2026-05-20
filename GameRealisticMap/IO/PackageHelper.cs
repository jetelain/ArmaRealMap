using System.Text.Json;
using System.Text.Json.Serialization;
using Pmad.HugeImages;
using Pmad.HugeImages.IO;
using Pmad.HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.IO
{
    internal static class PackageHelper
    {
        private static JsonSerializerOptions DefaultOptions = new JsonSerializerOptions {
            Converters = {
                new JsonStringEnumConverter()
            }
        };

        public static async ValueTask<T> ReadJson<T>(this IPackageReader package, string filename, JsonSerializerOptions? options = null)
        {
            using (var stream = package.ReadFile(filename))
            {
                var result = await JsonSerializer.DeserializeAsync<T>(stream, options ?? DefaultOptions);
                if (result == null)
                {
                    throw new JsonException("Invalid JSON file.");
                }
                return result!;
            }
        }

        public static async Task WriteJson<T>(this IPackageWriter package, string filename, T data, JsonSerializerOptions? options = null)
        {
            using (var stream = package.CreateFile(filename))
            {
                await JsonSerializer.SerializeAsync(stream, data, options ?? DefaultOptions);
            }
        }


        public static async Task WriteHugeImage<TPixel>(this IPackageWriter package, string filename, HugeImage<TPixel> data)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var stream = package.CreateFile(filename))
            {
                await data.SaveAsync(stream);
            }
        }

        public static async Task<HugeImage<TPixel>> ReadHugeImage<TPixel>(this IPackageReader package, string filename, IHugeImageStorage storage, string name)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using (var stream = package.ReadFile(filename))
            {
                var copyStorage = storage as IHugeImageStorageCanCopy;
                if (copyStorage != null)
                {
                    return await HugeImageIO.LoadCopyAsync<TPixel>(stream, copyStorage, name);
                }
                return await HugeImageIO.LoadCloneAsync<TPixel>(stream, storage, name);
            }
        }
    }
}
