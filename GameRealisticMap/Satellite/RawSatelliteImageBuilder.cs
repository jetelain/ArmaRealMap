using GameRealisticMap.Configuration;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using Pmad.HugeImages;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Satellite
{
    internal class RawSatelliteImageBuilder : IDataBuilderAsync<RawSatelliteImageData>, IDataSerializer<RawSatelliteImageData>
    {
        private readonly ISourceLocations sources;

        public RawSatelliteImageBuilder(ISourceLocations sources)
        {
            this.sources = sources;
        }

        public async Task<RawSatelliteImageData> BuildAsync(IBuildContext context, IProgressScope scope)
        {
            var totalSize = (int)Math.Ceiling(context.Area.SizeInMeters / context.Options.Resolution);

            var himage = new HugeImage<Rgba32>(context.HugeImageStorage, nameof(RawSatelliteImageBuilder), new Size(totalSize));
            using (var report2 = scope.CreateInteger(SatelliteImageProvider.GetName(sources), himage.Parts.Sum(t => t.RealRectangle.Height)))
            {
                using var src = new SatelliteImageProvider(report2, sources);
                foreach (var part in himage.Parts)
                {
                    await LoadPart(context, totalSize, part, report2, src).ConfigureAwait(false);
                    await himage.OffloadAsync().ConfigureAwait(false);
                }
            }

            return new RawSatelliteImageData(himage);
        }

        private async Task LoadPart(IBuildContext context, int totalSize, HugeImagePart<Rgba32> part, IProgressInteger report, SatelliteImageProvider src)
        {
            var imageryResolution = context.Options.Resolution;
            var options = context.Options.Satellite;

            using var token = await part.AcquireAsync().ConfigureAwait(false);

            var img = token.GetImageReadWrite();

            var parallel = 16;
            var dh = part.RealRectangle.Height / parallel;

            await Parallel.ForEachAsync(Enumerable.Range(0, parallel), async (dy, _) =>
            {
                var stX = part.RealRectangle.X;
                var stY = part.RealRectangle.Y;
                var endX = part.RealRectangle.Right;
                var y1 = part.RealRectangle.Y + (dy * dh);
                var y2 = dy == parallel - 1 ? part.RealRectangle.Bottom : part.RealRectangle.Y + ((dy + 1) * dh);
                for (int ry = y1; ry < y2; ry++)
                {
                    for (int rx = part.RealRectangle.X; rx < endX; rx++)
                    {
                        var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint((float)(rx * imageryResolution), (float)((totalSize - ry - 1) * imageryResolution)));
                        img[rx - stX, ry - stY] = await src.GetPixel(latLong).ConfigureAwait(false);
                    }
                    report.ReportOneDone();
                }
            }).ConfigureAwait(false);

            img.Mutate(d => d.GaussianBlur(1f));

            if ( options.Brightness != 1f || options.Contrast != 1f || options.Saturation != 1f)
            {
                img.Mutate(d => d.Brightness(options.Brightness).Contrast(options.Contrast).Saturate(options.Saturation));
            }

        }

        public async ValueTask<RawSatelliteImageData> Read(IPackageReader package, IContext context)
        {
            return new RawSatelliteImageData(await package.ReadHugeImage<Rgba32>("RawSatellite.himg.zip", context.HugeImageStorage, nameof(RawSatelliteImageBuilder)));
        }

        public async Task Write(IPackageWriter package, RawSatelliteImageData data)
        {
            await package.WriteHugeImage("RawSatellite.himg.zip", data.Image);
        }
    }
}
