using System.Windows.Media.Imaging;
using GameRealisticMap.Studio.Toolkit;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    internal static class ColorFix
    {
        public static BitmapSource ToArma3(BitmapSource source)
        {
            using var img = source.ToImageSharp<Bgra32>();
            Arma3ColorRender.Mutate(img, Arma3ColorRender.ToArma3);
            return img.ToWpf();
        }

        public static BitmapSource FromArma3(BitmapSource source)
        {
            using var img = source.ToImageSharp<Bgra32>();
            Arma3ColorRender.Mutate(img, Arma3ColorRender.FromArma3);
            return img.ToWpf();
        }
    }
}
