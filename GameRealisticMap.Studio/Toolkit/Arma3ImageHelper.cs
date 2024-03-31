using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace GameRealisticMap.Studio.Toolkit
{
    internal static class Arma3ImageHelper
    {
        internal static int[] ValidSizes = new[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };

        public static bool IsValidSize(BitmapFrame image)
        {
            return image.PixelWidth == image.PixelHeight && ValidSizes.Contains(image.PixelWidth);
        }
    }
}
