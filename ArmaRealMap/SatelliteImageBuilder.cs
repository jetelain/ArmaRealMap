using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoordinateSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap
{
    class SatelliteImageBuilder
    {
        internal static void BuildSatImage(AreaInfos area)
        {
            var bd = new BdOrtho();
            bd.Preload(area);

            var report = new ProgressReport("BuildSatImage", area.Size * area.CellSize * area.Size * area.CellSize);
            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize))
            {
                var startPointUTM = area.StartPointUTM;
                var eager = new EagerLoad(false);
                var done = 0;
                Parallel.For(0, img.Height, y =>
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        var point = new UniversalTransverseMercator(
                                startPointUTM.LatZone,
                                startPointUTM.LongZone,
                                startPointUTM.Easting + (double)x,
                                startPointUTM.Northing + (double)y);

                        var latLong = UniversalTransverseMercator.ConvertUTMtoLatLong(point, eager);
                        img[x, img.Height - y - 1] = bd.GetPixel(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble());
                        report.ReportItemsDone(Interlocked.Increment(ref done));
                    }
                });
                report.TaskDone();
                Console.WriteLine("SavePNG");
                img.Save("sat2.png");
            }


        }
    }
}
