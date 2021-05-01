using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using FreeImageAPI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap
{
    public class BdOrtho
    {
        private readonly string[] allImages;
        private readonly ConcurrentDictionary<string, Image<Rgb24>> cache = new ConcurrentDictionary<string, Image<Rgb24>>();
        private readonly object fileSystemCacheLock = new object();

        public BdOrtho()
        {
            this.allImages = Directory.GetFiles(@"E:\Carto\BDORTHO\", "*.jp2", SearchOption.AllDirectories);
        }

        public Rgb24 GetPixel(double latitude, double longitude)
        {
            var p = LambertHelper.WGS84ToLambert93(latitude, longitude);

            var tileX = (((int)p[0] / 5000) * 5);
            var tileY = (((int)p[1] / 5000) * 5);

            var img = GetTile(tileX, tileY);

            var x = ((int)p[0] - (tileX * 1000)) * 2;
            var y = 9999 - (((int)p[1] - (tileY * 1000)) * 2);

            return img[x, y];
        }

        private Image<Rgb24> GetTile(int tileX, int tileY)
        {
            var fileSuffix = $"-{tileX:0000}-{tileY + 5:0000}-LA93-0M50-E080.jp2";
            var tileFile = allImages.FirstOrDefault(p => p.EndsWith(fileSuffix, StringComparison.OrdinalIgnoreCase));
            Image<Rgb24> img;
            if (!cache.TryGetValue(tileFile, out img))
            {
                var plainPng = Path.ChangeExtension(tileFile, ".png");
                lock (fileSystemCacheLock)
                {
                    if (!File.Exists(plainPng))
                    {
                        var jpeg2000 = FreeImage.LoadEx(tileFile);
                        FreeImage.SaveEx(ref jpeg2000, plainPng, FREE_IMAGE_SAVE_FLAGS.PNG_Z_DEFAULT_COMPRESSION, true);
                    }
                }
                img = Image.Load<Rgb24>(plainPng);
                cache.TryAdd(tileFile, img);
            }
            return img;
        }

        internal void Preload(AreaInfos areaInfos)
        {
            var points = new[]
            {
                LambertHelper.WGS84ToLambert93(areaInfos.NorthWest),
                LambertHelper.WGS84ToLambert93(areaInfos.SouthWest),
                LambertHelper.WGS84ToLambert93(areaInfos.NorthEast),
                LambertHelper.WGS84ToLambert93(areaInfos.SouthEast)
            };

            var minTileX = ((points.Min(p => (int)p[0]) / 5000) * 5);
            var maxTileX = ((points.Max(p => (int)p[0]) / 5000) * 5);
            var minTileY = ((points.Min(p => (int)p[1]) / 5000) * 5);
            var maxTileY = ((points.Max(p => (int)p[1]) / 5000) * 5);

            var report = new ProgressReport("PreloadBdOrtho", (((maxTileX - minTileX) / 5) + 1) * (((maxTileY - minTileY) / 5) + 1));
            for (var tileX = minTileX; tileX <= maxTileX; tileX += 5)
            {
                for (var tileY = minTileY; tileY <= maxTileY; tileY += 5)
                {
                    GetTile(tileX, tileY);
                    report.ReportOneDone();
                }
            }
            report.TaskDone();
        }


    }
    /*

(900000.00,6690000.00) (    0,    0) Label "Pt 1",
(905000.00,6690000.00) (10000,    0) Label "Pt 2",
(900000.00,6685000.00) (    0,10000) Label "Pt 4"
(905000.00,6685000.00) (10000,10000) Label "Pt 3",

 */
}
