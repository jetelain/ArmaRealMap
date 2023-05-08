using GameRealisticMap.ManMade.Roads.Libraries;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Assets
{
    internal interface IArma3RoadTypeInfos : IRoadTypeInfos
    {
        Color SatelliteColor { get; }
        float TextureWidth { get; }
        string Texture { get; }
        string TextureEnd { get; }
        string Material { get; }
    }
}
