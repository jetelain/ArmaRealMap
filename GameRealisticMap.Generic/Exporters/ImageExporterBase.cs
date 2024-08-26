using HugeImages;
using HugeImages.IO;
using HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class ImageExporterBase<TPixel> : IExporter
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public abstract string Name { get; }

        public IEnumerable<ExportFormat> Formats => [ExportFormat.Image, ExportFormat.HugeImage];

        public Task Export(string filename, ExportFormat format, IBuildContext context, IDictionary<string, object>? properties)
        {
            switch (format)
            {
                case ExportFormat.Image:
                    return ExportSingleImage(context, filename);

                case ExportFormat.HugeImage:
                    return ExportHugeImage(context, filename);
            }
            return Task.FromException(new ApplicationException($"Format '{format}' is not supported by '{Name}'"));
        }

        protected abstract ValueTask<HugeImage<TPixel>> GetImage(IBuildContext context);

        private async Task ExportSingleImage(IBuildContext context, string filename)
        {
            var himg = await GetImage(context);
            await himg.SaveUniqueAsync(filename);
        }


        private async Task ExportHugeImage(IBuildContext context, string filename)
        {
            var himg = await GetImage(context);
            await himg.SaveAsync(filename);
        }
    }
}