using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.GameEngine
{
    public interface ITerrainMaterialLibrary
    {
        ITerrainMaterial GetMaterial(Rgb24 color);
    }
}