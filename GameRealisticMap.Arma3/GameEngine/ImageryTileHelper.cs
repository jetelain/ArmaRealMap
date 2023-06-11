using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal static class ImageryTileHelper
    {
        internal static void FillEdges(Size source, int x, int num, Image<Rgb24> tile, int y, Point pos)
        {
            if (x == 0)
            {
                FillX(tile, pos.X, -1);
            }
            else if (x == num - 1)
            {
                FillX(tile, pos.X + source.Width - 1, +1);
            }
            if (y == 0)
            {
                FillY(tile, pos.Y, -1);
            }
            else if (y == num - 1)
            {
                FillY(tile, pos.Y + source.Height - 1, +1);
            }
        }

        private static void FillY(Image<Rgb24> tile, int sourceY, int d)
        {
            var y = sourceY + d;
            while (y >= 0 && y < tile.Height)
            {
                for (int x = 0; x < tile.Width; ++x)
                {
                    tile[x, y] = tile[x, sourceY];
                }
                y += d;
            }
        }

        private static void FillX(Image<Rgb24> tile, int sourceX, int d)
        {
            var x = sourceX + d;
            while (x >= 0 && x < tile.Width)
            {
                for (int y = 0; y < tile.Height; ++y)
                {
                    tile[x, y] = tile[sourceX, y];
                }
                x += d;
            }
        }

    }
}
