using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using CoordinateSharp;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;
using OsmSharp.Tags;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SRTM;
using SRTM.Sources.NASA;

namespace ArmaRealMap
{
    class ElevationGridBuilder
    {
        internal static void BuildElevationGrid(ConfigSRTM configSRTM, AreaInfos area)
        {
            var credentials = new NetworkCredential(configSRTM.Login, configSRTM.Password);
            var srtmData = new SRTMData(configSRTM.Cache, new NASASource(credentials));
            var elevationMatrix = new double[area.Size, area.Size];
            var startPointUTM = area.StartPointUTM;
            var sw = Stopwatch.StartNew();
            var eager = new EagerLoad(false);
            var min = 4000d;
            var max = 0d;
            for (int y = 0; y < area.Size; y++)
            {
                for (int x = 0; x < area.Size; x++)
                {
                    var point = new UniversalTransverseMercator(
                            startPointUTM.LatZone,
                            startPointUTM.LongZone,
                            startPointUTM.Easting + (double)(x * area.CellSize) + (double)area.CellSize / 2.0d,
                            startPointUTM.Northing + (double)(y * area.CellSize) + (double)area.CellSize / 2.0d);

                    var latLong = UniversalTransverseMercator.ConvertUTMtoLatLong(point, eager);

                    var elevation = srtmData.GetElevationBilinear(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble()) ?? double.NaN;
                    elevationMatrix[x, y] = elevation;
                    max = Math.Max(elevation, max);
                    min = Math.Min(elevation, min);
                }

                var value = y + 1;
                var percentDone = Math.Round((double)value * 100d / area.Size, 2);
                var milisecondsLeft = sw.ElapsedMilliseconds * (area.Size - value) / value;
                Console.WriteLine($"{percentDone}% - {Math.Ceiling(milisecondsLeft / 60000d)} min left");
            }
            sw.Stop();

            using (var img = new Image<Rgb24>(area.Size, area.Size))
            {
                for (int y = 0; y < area.Size; y++)
                {
                    for (int x = 0; x < area.Size; x++)
                    {
                        byte value = (byte)((elevationMatrix[x, y] - min) * 255 / (max - min));
                        img[x, area.Size - y - 1] = new Rgb24(value, value, value);
                    }
                }
                img.Save("elevation.png");
            }

            using (var writer = new StreamWriter(new FileStream("elevation.asc", FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine($"ncols         {area.Size}");
                writer.WriteLine($"nrows         {area.Size}");
                writer.WriteLine($"xllcorner     {startPointUTM.Easting:0}");
                writer.WriteLine($"yllcorner     {startPointUTM.Northing:0}");
                writer.WriteLine($"cellsize      {area.CellSize}");
                writer.WriteLine($"NODATA_value  -9999");

                for (int y = 0; y < area.Size; y++)
                {
                    for (int x = 0; x < area.Size; x++)
                    {
                        writer.Write(elevationMatrix[x, area.Size - y - 1].ToString("0.00", CultureInfo.InvariantCulture));
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                }
            }
        }
    }
}
