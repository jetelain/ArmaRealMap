using System.Numerics;
using GeoJSON.Text.Feature;
using Pmad.HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Satellite
{
    /// <summary>
    /// Raw Sentinel-2 cloudless satellite imagery assembled as a <see cref="Pmad.HugeImages.HugeImage{TPixel}"/>
    /// covering the full terrain area at the configured resolution. Stored memory-mapped
    /// to disk via <see cref="IContext.HugeImageStorage"/> because the image may exceed available RAM.
    /// </summary>
    public class RawSatelliteImageData
    {
        public RawSatelliteImageData(HugeImage<Rgba32> image)
        {
            Image = image;
        }

        public HugeImage<Rgba32> Image { get; }
    }
}
