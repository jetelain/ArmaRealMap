﻿using System.Threading;
using System.Threading.Tasks;
using ArmaRealMap.DataSources;
using ArmaRealMap.DataSources.S2C;
using CoordinateSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap
{
    class SatelliteImageBuilder
    {
        internal static void BuildSatImage(MapInfos area, string targetFile, Config config)
        {
            using (var src = CreateSource(area, config))
            {
                var report = new ProgressReport("BuildSatImage", area.ImageryWidth * area.ImageryHeight);
                using (var img = new Image<Rgb24>(area.ImageryWidth, area.ImageryHeight))
                {
                    var startPointUTM = area.StartPointUTM;
                    var eager = new EagerLoad(false);
                    var done = 0;
                    Parallel.For(0, img.Height, y =>
                    {
                        for (int x = 0; x < img.Width; x++)
                        {
                            //var point = new UniversalTransverseMercator(
                            //        startPointUTM.LatZone,
                            //        startPointUTM.LongZone,
                            //        startPointUTM.Easting + (x * area.ImageryResolution),
                            //        startPointUTM.Northing + (y * area.ImageryResolution));
                            //var latLong = UniversalTransverseMercator.ConvertUTMtoLatLong(point, eager);

                            var latLong = area.TerrainToLatLong(x * area.ImageryResolution, y * area.ImageryResolution);
                            img[x, img.Height - y - 1] = src.GetPixel(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble());
                            report.ReportItemsDone(Interlocked.Increment(ref done));
                        }
                    });
                    report.TaskDone();
                    DrawHelper.SavePngChuncked(img, targetFile);
                }
            }
        }

        private static ISatImageProvider CreateSource(MapInfos area, Config config)
        {
            if (config.ORTHO != null)
            {
                var bdOrtho = new BdOrtho();
                bdOrtho.Preload(area);
                return bdOrtho;
            }
            return new S2Cloudless(config.SharedCache);
        }
    }
}