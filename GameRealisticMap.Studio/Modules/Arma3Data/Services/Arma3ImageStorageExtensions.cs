using System.Windows.Media.Imaging;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal static class Arma3ImageStorageExtensions
    {
        public static void Save(this IArma3ImageStorage storage, string path, BitmapFrame img)
        {
            using (var stream = storage.CreatePng(path))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(img);
                encoder.Save(stream);
            }
        }
    }
}
