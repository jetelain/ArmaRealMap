using HugeImages;
using HugeImages.Storage;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Generic.Exporters
{
    internal abstract class ImageExporterBase<TPixel> : IExporter
        where TPixel : unmanaged, IPixel<TPixel>
    {
        public abstract string Name { get; }

        public IEnumerable<ExportFormat> Formats => [ExportFormat.Image];

        public Task Export(string filename, ExportFormat format, IBuildContext context)
        {
            switch (format)
            {
                case ExportFormat.Image:
                    return Export(context, filename);
            }
            return Task.FromException(new ApplicationException($"Format '{format}' is not supported by '{Name}'"));
        }

        protected abstract HugeImage<TPixel> GetImage(IBuildContext context);

        private async Task Export(IBuildContext context, string filename)
        {
            var himg = GetImage(context);
            await himg.SaveUniqueAsync(filename);
        }
    }
}