using GameRealisticMap.Arma3.GameEngine;
using Pmad.HugeImages;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    internal sealed class ImageryTilerHugeImagePartitioner : IHugeImagePartitioner, IImageryPartitioner
    {
        private readonly List<ImageryTile> parts;
        private readonly int multiplier = 1;

        public ImageryTilerHugeImagePartitioner(ImageryTiler imageryTiler, int multiplier)
        {
            this.multiplier = multiplier;
            parts = imageryTiler.All.OrderBy(i => i.X).ThenBy(i => i.Y).ToList();
        }

        public ImageryTilerHugeImagePartitioner(List<ImageryTile> parts, int multiplier)
        {
            this.multiplier = multiplier;
            this.parts = parts;
        }

        public List<HugeImagePartDefinition> CreateParts(Size size)
        {
            return parts.Select(i => new HugeImagePartDefinition(
                new Rectangle(i.ContentTopLeft * multiplier, GetSize(i.ContentBottomRight, i.ContentTopLeft) * multiplier),
                new Rectangle(i.ImageTopLeft * multiplier, GetSize(i.ImageBottomRight, i.ImageTopLeft) * multiplier)
                    )).ToList();
        }

        public ImageryTile GetPartFromId(int partId)
        {
            return parts[partId - 1];
        }

        private static Size GetSize(Point bottomRight, Point topLeft)
        {
            return new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }
    }
}
