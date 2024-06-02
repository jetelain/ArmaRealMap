using System.Numerics;
using System.Threading.Tasks;
using GameRealisticMap.Configuration;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.Reporting;
using HugeImages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Satellite
{
    internal class RawSatelliteImageBuilder : IDataBuilder<RawSatelliteImageData>, IDataSerializer<RawSatelliteImageData>
    {
        private readonly IProgressSystem progress;
        private readonly ISourceLocations sources;

        public RawSatelliteImageBuilder(IProgressSystem progress, ISourceLocations sources)
        {
            this.progress = progress;
            this.sources = sources;
        }

        public RawSatelliteImageData Build(IBuildContext context)
        {
            //Image<Rgb24> image;

            var totalSize = (int)Math.Ceiling(context.Area.SizeInMeters / context.Imagery.Resolution);

            //using (var report = progress.CreateStep("S2C OLD", totalSize /*tileSize * tileCount * tileCount*/))
            //{
            //    image = LoadImage(context, totalSize, report, Vector2.Zero, 0);
            //    image.SaveAsPng(@"c:\temp\test.png");
            //}

            var himage = new HugeImage<Rgba32>(context.HugeImageStorage, nameof(RawSatelliteImageBuilder), new Size(totalSize));
            using (var report2 = progress.CreateStep("S2C", himage.Parts.Sum(t => t.RealRectangle.Height)))
            {
                using var src = new S2Cloudless(progress, sources);
                foreach (var part in himage.Parts)
                {
                    LoadPart(context, totalSize, part, report2, src).Wait();
                    himage.OffloadAsync().Wait();
                }
            }

            return new RawSatelliteImageData(himage);
        }

        private async Task LoadPart(IBuildContext context, int totalSize, HugeImagePart<Rgba32> part, IProgressInteger report, S2Cloudless src)
        {
            var imageryResolution = context.Imagery.Resolution;

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
        }

        public async ValueTask<RawSatelliteImageData> Read(IPackageReader package, IContext context)
        {
            //var image = await Image.LoadAsync<Rgb24>(package.ReadFile("RawSatellite.png"), new PngDecoder());
            //return new RawSatelliteImageData(image);
            throw new NotImplementedException();
        }

        public async Task Write(IPackageWriter package, RawSatelliteImageData data)
        {
            //using(var stream = package.CreateFile("RawSatellite.png"))
            //{
            //    await data.Image.SaveAsPngAsync(stream);
            //}
            throw new NotImplementedException();
        }

        private Image<Rgba32> LoadImage(IBuildContext context, int tileSize, IProgressInteger report, Vector2 start, int done)
        {
            var imageryResolution = context.Imagery.Resolution;
            using var src = new S2Cloudless(progress, sources);
            var img = new Image<Rgba32>(tileSize, tileSize);
            var parallel = 16;
            var dh = img.Height / parallel;
            Parallel.For(0, parallel, dy =>
            {
                var y1 = dy * dh;
                var y2 = (dy + 1) * dh;
                for (int y = y1; y < y2; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint((float)(x * imageryResolution), (float)(y * imageryResolution)) + start);
                        img[x, img.Height - y - 1] = src.GetPixel(latLong).Result;
                    }
                    report.Report(Interlocked.Increment(ref done));
                }
            });
            img.Mutate(d => d.GaussianBlur(1f));
            return img;
        }
    }
}
