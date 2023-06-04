using HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IImagerySource
    {
        HugeImage<Rgba32> CreateIdMap();

        Image CreatePictureMap();

        HugeImage<Rgba32> CreateSatMap();

        Image CreateSatOut();
    }
}
