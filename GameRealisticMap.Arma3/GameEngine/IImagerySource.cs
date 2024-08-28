using HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IImagerySource
    {
        Task<HugeImage<Rgba32>> CreateIdMap();

        Task<Image> CreatePictureMap();

        Task<HugeImage<Rgba32>> CreateSatMap();

        Image CreateSatOut();
    }
}
