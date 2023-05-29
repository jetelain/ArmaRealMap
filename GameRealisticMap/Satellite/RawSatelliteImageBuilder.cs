using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.IO;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Satellite
{
    internal class RawSatelliteImageBuilder : IDataBuilder<RawSatelliteImageData>, IDataSerializer<RawSatelliteImageData>
    {
        private readonly IProgressSystem progress;

        public RawSatelliteImageBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public RawSatelliteImageData Build(IBuildContext context)
        {
            var totalSize = (int)Math.Ceiling(context.Area.SizeInMeters / context.Imagery.Resolution);


            /*var tileSize = totalSize;
            var tileCount = 1;
            while (tileSize > maxTileSize)
            {
                tileSize /= 2;
                tileCount *= 2;
            }*/

            using var report = progress.CreateStep("S2C", totalSize /*tileSize * tileCount * tileCount*/);

            /* Tiling might be required for heavy maps, but makes everything much more complex
             * 
            var list = new List<ImageTile>();
            var done = 0;
            for (var tx = 0; tx < tileCount; tx++)
            {
                for (var ty = 0; ty < tileCount; ty++)
                {
                    var offset = new Vector2(tx * tileSize, ty * tileSize);
                    var image = LoadImage(context, tileSize, src, report, offset, done);
                    list.Add(new ImageTile(image, offset));
                    done += tileSize;
                }
            }*/

            var image = LoadImage(context, totalSize, report, Vector2.Zero, 0);

            return new RawSatelliteImageData(image);
        }

        public async ValueTask<RawSatelliteImageData> Read(IPackageReader package, IContext context)
        {
            var image = await Image.LoadAsync<Rgb24>(package.ReadFile("RawSatellite.png"), new PngDecoder());
            return new RawSatelliteImageData(image);
        }

        public async Task Write(IPackageWriter package, RawSatelliteImageData data)
        {
            using(var stream = package.CreateFile("RawSatellite.png"))
            {
                await data.Image.SaveAsPngAsync(stream);
            }
        }

        private Image<Rgb24> LoadImage(IBuildContext context, int tileSize, IProgressInteger report, Vector2 start, int done)
        {
            var imageryResolution = context.Imagery.Resolution;
            using var src = new S2Cloudless(progress);
            var img = new Image<Rgb24>(tileSize, tileSize);
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
