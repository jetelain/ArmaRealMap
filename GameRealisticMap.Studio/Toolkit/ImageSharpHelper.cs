using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
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

        internal static Image<TPixel> ToImageSharp<TPixel>(this BitmapSource bitmapFrame)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var image = ToImageSharp(bitmapFrame);
            return image.CloneAs<TPixel>();
        }

        internal static Image ToImageSharp(this BitmapSource bitmapFrame)
        {
            if (bitmapFrame.Format == PixelFormats.Bgra32)
            {
                return ConvertFromBitmap<Bgra32>(bitmapFrame);
            }
            if (bitmapFrame.Format == PixelFormats.Bgr24)
            {
                return ConvertFromBitmap<Bgr24>(bitmapFrame);
            }
            if (bitmapFrame.Format == PixelFormats.Rgb24)
            {
                return ConvertFromBitmap<Rgb24>(bitmapFrame);
            }
            var converted = new FormatConvertedBitmap();
            converted.BeginInit();
            converted.Source = bitmapFrame;
            converted.DestinationFormat = PixelFormats.Bgra32;
            converted.EndInit();
            return ToImageSharp(converted);
        }

        private static Image<TPixel> ConvertFromBitmap<TPixel>(BitmapSource bitmap)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            var sizeOfPixel = Marshal.SizeOf<TPixel>();
            var pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * sizeOfPixel];
            bitmap.CopyPixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), pixels, bitmap.PixelWidth * sizeOfPixel, 0);
            return Image.LoadPixelData<TPixel>(pixels, bitmap.PixelWidth, bitmap.PixelHeight);
        }

        internal static BitmapSource ToWpf<TPixel>(this Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel>
        {
            using var clone = image.CloneAs<Bgra32>();
            return ToWpf(clone);
        }

        internal static BitmapSource ToWpf(this Image<Bgra32> image)
        {
            var buffer = new byte[image.Width * image.Height * 4];
            image.CopyPixelDataTo(new Span<byte>(buffer));
            return BitmapSource.Create(image.Width, image.Height, 300, 300, PixelFormats.Bgra32, null, buffer, image.Width * 4);
        }
    }
}
