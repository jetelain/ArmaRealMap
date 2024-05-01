using System.Windows.Media.Imaging;
using GameRealisticMap.Arma3;

namespace GameRealisticMap.Studio.Toolkit
{
    internal static class Arma3ImageHelper
    {
        public static bool IsValidSize(BitmapSource image)
        {
            return image.PixelWidth == image.PixelHeight && Arma3ConfigHelper.IsValidImageSize(image.PixelWidth, image.PixelHeight);
        }
    }
}
