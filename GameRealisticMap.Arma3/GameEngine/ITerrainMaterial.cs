using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface ITerrainMaterial
    {
        string NormalTexture { get; }

        string ColorTexture { get; }

        Rgb24 Id { get; }
    }
}