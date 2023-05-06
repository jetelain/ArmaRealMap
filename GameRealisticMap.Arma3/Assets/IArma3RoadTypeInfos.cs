using GameRealisticMap.ManMade.Roads.Libraries;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Assets
{
    internal interface IArma3RoadTypeInfos : IRoadTypeInfos
    {
        Color SatelliteColor { get; }
    }
}
