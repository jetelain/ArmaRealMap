using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Toolkit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = System.Windows.Media.Color;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Services
{
    internal static class GdtHelper
    {
        internal static Color AllocateUniqueColor(Color? avgColor, IEnumerable<Color> usedColors)
        {
            Color wanted;

            if (avgColor != null)
            {
                var hsl = ColorSpaceConverter.ToHsl(new Rgb24(avgColor.Value.R, avgColor.Value.G, avgColor.Value.B));
                wanted = new Hsl(hsl.H, 1f, Math.Clamp(hsl.L, 0.25f, 0.75f)).ToWpfColor();
            }
            else
            {
                wanted = RandomColor();
            }
            int attempt = 1;
            while (usedColors.Contains(wanted))
            {
                wanted = RandomColor(attempt);
                attempt++;
            }
            return wanted;
        }

        private static Color RandomColor(int attempt = 0)
        {
            if (attempt < 3)
            {
                return new Hsl(Random.Shared.Next(0, 72) * 5, 1f, 0.5f).ToWpfColor();
            }
            return Color.FromRgb((byte)Random.Shared.Next(64, 192), (byte)Random.Shared.Next(64, 192), (byte)Random.Shared.Next(64, 192));
        }

        internal static Image<Rgb24>? GenerateFakeSatPngImage(IArma3Previews preview, string colorTexture)
        {
            var uri = preview.GetTexturePreview(colorTexture);
            if (uri != null && uri.IsFile)
            {
                var img = Image.Load<Rgb24>(uri.LocalPath);
                img.Mutate(d =>
                {
                    d.Resize(1, 1);
                    d.Resize(8, 8);
                });
                return img;
            }
            return null;
        }
    }
}
