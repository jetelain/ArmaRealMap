using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Satellite
{
    internal class RawSatelliteImageBuilder : IDataBuilder<RawSatelliteImageData>
    {
        private readonly IProgressSystem progress;
        private readonly float imageryResolution = 1;
        // private const int maxTileSize = 8192; // ~200 Mo with 24bits/pixel

        public RawSatelliteImageBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public RawSatelliteImageData Build(IBuildContext context)
        {
            var totalSize = (int)Math.Ceiling(context.Area.SizeInMeters * imageryResolution);


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

        private Image<Rgb24> LoadImage(IBuildContext context, int tileSize, IProgressInteger report, Vector2 start, int done)
        {
            using var src = new S2Cloudless();
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
                        var latLong = context.Area.TerrainPointToLatLng(new TerrainPoint(x * imageryResolution, y * imageryResolution) + start);
                        img[x, img.Height - y - 1] = src.GetPixel(latLong).Result;
                    }
                    report.Report(Interlocked.Increment(ref done));
                }
            });
            img.Mutate(d => d.GaussianBlur(2.2f));
            return img;
        }
    }
}
