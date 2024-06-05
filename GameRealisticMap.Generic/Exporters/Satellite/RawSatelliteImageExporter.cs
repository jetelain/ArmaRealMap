using GameRealisticMap.Satellite;
using HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Generic.Exporters.Satellite
{
    internal class RawSatelliteImageExporter : ImageExporterBase<Rgba32>
    {
        public override string Name => "RawSatelliteImage";

        protected override HugeImage<Rgba32> GetImage(IBuildContext context)
        {
            return context.GetData<RawSatelliteImageData>().Image;
        }
    }
}
