using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Toolkit
{
    internal static class ImageSharpHelper
    {
        internal static System.Windows.Media.Color ToWpfColor(this Rgb24 px)
        {
            return System.Windows.Media.Color.FromRgb(px.R, px.G, px.B);
        }

        internal static Rgb24 ToRgb24(this System.Windows.Media.Color px)
        {
            return new Rgb24(px.R, px.G, px.B);
        }

        internal static byte[] ToPngByteArray(this Image img)
        {
            var mem = new MemoryStream();
            img.SaveAsPng(mem);
            return mem.ToArray();
        }
        internal static System.Windows.Media.Color ToWpfColor(this Hsl hsl)
        {
            var rgb = (Rgb24)ColorSpaceConverter.ToRgb(hsl);
            return System.Windows.Media.Color.FromRgb(rgb.R, rgb.G, rgb.B);
        }
    }
}
