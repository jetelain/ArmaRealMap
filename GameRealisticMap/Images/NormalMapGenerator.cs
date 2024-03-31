using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Images
{
    public class NormalMapGenerator
    {
        public static Image<Rgba32> FromHeightBumpMap(Image<Rgba32> source, double extrusion = 2.0)
        {
            var normalImage = new Image<Rgba32>(source.Width, source.Height);

            for (var y = 0; y < source.Height; y++)
            {
                for (var x = 0; x < source.Width; x++)
                {
                    double up = GetPixelWrapped(source, x, y - 1);
                    double down = GetPixelWrapped(source, x, y + 1);
                    double left = GetPixelWrapped(source, x - 1, y);
                    double right = GetPixelWrapped(source, x + 1, y);
                    double upleft = GetPixelWrapped(source, x - 1, y - 1);
                    double upright = GetPixelWrapped(source, x + 1, y - 1);
                    double downleft = GetPixelWrapped(source, x - 1, y + 1);
                    double downright = GetPixelWrapped(source, x + 1, y + 1);

                    double vert = (down - up) * 2.0 + downright + downleft - upright - upleft;
                    double horiz = (right - left) * 2.0 + upright + downright - upleft - downleft;
                    double depth = 1.0 / extrusion;
                    double scale = 127.0 / Math.Sqrt(vert * vert + horiz * horiz + depth * depth);

                    normalImage[x, y] = new Rgba32(
                        r: (byte)(128 - horiz * scale), 
                        g: (byte)(128 - vert * scale), // DX (revert to get GL version)
                        b: (byte)(128 + depth * scale), 
                        a: 255);
                }
            }
            return normalImage;
        }

        private static double GetPixelWrapped(Image<Rgba32> source, int x, int y)
        {
            if (x < 0) { x += source.Width; }
            if (y < 0) { y += source.Height; }
            if (x >= source.Width)  { x -= source.Width;  }
            if (y >= source.Height) { y -= source.Height; }
            var rgb = source[x, y];
            return (rgb.R + rgb.G + rgb.B) / (255 * 3.0);
        }

    }
}
