using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal static class Arma3ImageStorageExtensions
    {
        public static void SavePng(this IArma3ImageStorage storage, string path, BitmapFrame img)
        {
            using (var stream = storage.CreatePng(path))
            {
                using var imgs = img.ToImageSharp();
                imgs.SaveAsPng(stream, new PngEncoder() { ColorType = PngColorType.RgbWithAlpha, BitDepth = PngBitDepth.Bit8 });
            }
        }

        public static void ProcessPngToPaaBackground(this IArma3ImageStorage storage)
        {
            ProcessPngToPaaBackground(storage, IoC.Get<IProgressTool>());
        }

        public static void ProcessPngToPaaBackground(this IArma3ImageStorage storage, IProgressTool progress)
        {
            if (storage.HasToProcessPngToPaa)
            {
                progress.RunTask("Png->PAA", p => storage.ProcessPngToPaa(p.Scope), false);
            }
        }
    }
}
