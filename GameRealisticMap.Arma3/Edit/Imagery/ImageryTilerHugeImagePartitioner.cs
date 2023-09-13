using GameRealisticMap.Arma3.GameEngine;
using HugeImages;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    internal sealed class ImageryTilerHugeImagePartitioner : IHugeImagePartitioner
    {
        private readonly List<ImageryTile> parts;

        public ImageryTilerHugeImagePartitioner(ImageryTiler imageryTiler)
        {
            parts = imageryTiler.All.OrderBy(i => i.X).ThenBy(i => i.Y).ToList();
        }

        public List<HugeImagePartDefinition> CreateParts(Size size)
        {
            return parts.Select(i => new HugeImagePartDefinition(
                new Rectangle(i.ContentTopLeft, GetSize(i.ContentBottomRight, i.ContentTopLeft)),
                new Rectangle(i.ImageTopLeft, GetSize(i.ImageBottomRight, i.ImageTopLeft))

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
