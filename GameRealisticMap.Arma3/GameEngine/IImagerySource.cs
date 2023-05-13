using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface IImagerySource
    {
        Image CreateIdMap();

        Image CreatePictureMap();

        Image CreateSatMap();

        Image CreateSatOut();
    }
}
