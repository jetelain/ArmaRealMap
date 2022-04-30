using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArmaRealMap.DataSources.S2C;
using CoordinateSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap
{
    class SatelliteImageBuilder
    {
        internal static void BuildSatImage(MapInfos area, string targetFile, GlobalConfig config)
        {
            using (var src = new S2Cloudless(config.S2C.CacheLocation))
            {
                var report = new ProgressReport("BuildSatImage", area.ImageryWidth * area.ImageryHeight);
                using (var img = new Image<Rgb24>(area.ImageryWidth, area.ImageryHeight))
                {
                    var startPointUTM = area.StartPointUTM;
                    var eager = new EagerLoad(false);
                    var done = 0;
                    var dh = img.Height / 128;

                    Parallel.For(0, 128, dy =>
                    {
                        var y1 = dy * dh;
                        var y2 = (dy + 1) * dh;
                        for (int y = y1; y < y2; y++)
                        {
                            for (int x = 0; x < img.Width; x++)
                            {
                                var latLong = area.TerrainToLatLong(x * area.ImageryResolution, y * area.ImageryResolution);
                                img[x, img.Height - y - 1] = src.GetPixel(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble());
                                report.ReportItemsDone(Interlocked.Increment(ref done));
                            }
                        }
                    });

                    //Parallel.For(0, img.Height, y =>
                    //{
                    //    for (int x = 0; x < img.Width; x++)
                    //    {
                    //        var latLong = area.TerrainToLatLong(x * area.ImageryResolution, y * area.ImageryResolution);
                    //        img[x, img.Height - y - 1] = src.GetPixel(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble());
                    //        report.ReportItemsDone(Interlocked.Increment(ref done));
                    //    }
                    //});

                    report.TaskDone();

                    using (new ProgressReport(Path.GetFileName(targetFile)))
                    {
                        img.Save(targetFile);
                    }
                }
            }
        }
    }
}
