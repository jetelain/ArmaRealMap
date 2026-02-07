using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using Pmad.HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public interface IImageryInfos
    {
        HugeImage<Rgb24> GetIdMap(IGameFileSystem fileSystem, TerrainMaterialLibrary materials);

        HugeImage<Rgb24> GetSatMap(IGameFileSystem fileSystem);

        double Resolution { get; }

        int IdMapMultiplier { get; }

        int TotalSize { get; }
    }
}
