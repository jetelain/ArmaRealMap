using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Satellite
{
    public class ImageTile
    {
        public ImageTile(Image<Rgb24> image, Vector2 offset)
        {
            Image = image;
            Offset = offset;
        }

        public Image<Rgb24> Image { get; }

        public Vector2 Offset { get; }
    }
}