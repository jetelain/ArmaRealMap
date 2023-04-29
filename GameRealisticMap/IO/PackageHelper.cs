using System.Text.Json;
using System.Text.Json.Serialization;

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
    }
}
