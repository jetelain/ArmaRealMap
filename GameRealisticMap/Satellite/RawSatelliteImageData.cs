using System.Numerics;
using GeoJSON.Text.Feature;
using Pmad.HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Satellite
{
    public class RawSatelliteImageData
    {
        public RawSatelliteImageData(HugeImage<Rgba32> image)
        {
            Image = image;
        }

        public HugeImage<Rgba32> Image { get; }
    }
}
