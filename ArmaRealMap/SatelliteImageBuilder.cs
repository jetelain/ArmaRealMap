using System;
using System.Diagnostics;
using CoordinateSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap
{
    class SatelliteImageBuilder
    {
        internal static void BuildImage(AreaInfos area)
        {
            var bd = new BdOrtho();

            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize))
            {
                var startPointUTM = area.StartPointUTM;
                var eager = new EagerLoad(false);
                var sw = Stopwatch.StartNew();
                for (int y = 0; y < img.Height; y++)
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
                    }
                    var value = y + 1;
                    var percentDone = Math.Round((double)value * 100d / img.Height, 2);
                    var milisecondsLeft = sw.ElapsedMilliseconds * (img.Height - value) / value;
                    Console.WriteLine($"{percentDone}% - {Math.Ceiling(milisecondsLeft / 60000d)} min left");
                }
                sw.Stop();
                img.Save("sat.png");
            }


        }
    }
}
