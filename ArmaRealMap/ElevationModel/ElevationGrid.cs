using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CoordinateSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SRTM;
using SRTM.Sources.NASA;

namespace ArmaRealMap.ElevationModel
{
    public class ElevationGrid
    {
        private readonly AreaInfos area;
        private readonly double[,] elevationMatrix;

        internal ElevationGrid(AreaInfos areaInfos)
        {
            area = areaInfos;
            elevationMatrix = new double[area.Size, area.Size];
        }

        public void LoadFromSRTM(ConfigSRTM configSRTM)
        {
            var credentials = new NetworkCredential(configSRTM.Login, configSRTM.Password);
            var srtmData = new SRTMData(configSRTM.Cache, new NASASource(credentials));
            var startPointUTM = area.StartPointUTM;
            //var sw = Stopwatch.StartNew();
            var eager = new EagerLoad(false);

            var done = 0;

            var delta = (double)area.CellSize / 2.0d;

            PreloadSRTM(srtmData);

            var report = new ProgressReport("LoadFromSRTM", area.Size);
            Parallel.For(0, area.Size, y =>
            {
                for (int x = 0; x < area.Size; x++)
                {
                    var point = new UniversalTransverseMercator(
                            startPointUTM.LatZone,
                            startPointUTM.LongZone,
                            startPointUTM.Easting + (double)(x * area.CellSize) + delta,
                            startPointUTM.Northing + (double)(y * area.CellSize) + delta);

                    var latLong = UniversalTransverseMercator.ConvertUTMtoLatLong(point, eager);

                    elevationMatrix[x, y] = srtmData.GetElevationBilinear(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble()) ?? double.NaN;
                }

                //var value = Interlocked.Increment(ref done);
                //var percentDone = Math.Round((double)value * 100d / area.Size, 2);
                //var milisecondsLeft = sw.ElapsedMilliseconds * (area.Size - value) / value;
                //Console.WriteLine($"{percentDone}% - {Math.Ceiling(milisecondsLeft / 60000d)} min left");
                report.ReportItemsDone(Interlocked.Increment(ref done));
            });

            report.TaskDone();
        }

        private void PreloadSRTM(SRTMData srtmData)
        {
            var report = new ProgressReport("PreloadSRTM", 4);
            srtmData.PreloadCell(area.NorthEast);
            report.ReportOneDone();
            srtmData.PreloadCell(area.SouthEast);
            report.ReportOneDone();
            srtmData.PreloadCell(area.NorthWest);
            report.ReportOneDone();
            srtmData.PreloadCell(area.NorthWest);
            report.TaskDone();
        }

        public void LoadFromAsc(string path)
        {
            var report = new ProgressReport("LoadFromAsc", area.Size);
            using (var reader = File.OpenText(path))
            {
                for(int i = 0;i < 6; ++i)
                {
                    reader.ReadLine();
                }
                string line;
                var y = area.Size - 1;
                while ((line = reader.ReadLine()) != null)
                {
                    int x = 0;
                    foreach(var item in line.Split(' ').Take(area.Size))
                    {
                        elevationMatrix[x, y] = double.Parse(item, CultureInfo.InvariantCulture);
                        x++;
                    }
                    report.ReportItemsDone(area.Size - y);
                    y--;
                }
            }
            report.TaskDone();
        }

        public void SaveToAsc(string path)
        {
            var report = new ProgressReport("SaveToAsc", area.Size);

            using (var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine($"ncols         {area.Size}");
                writer.WriteLine($"nrows         {area.Size}");
                writer.WriteLine($"xllcorner     {area.StartPointUTM.Easting:0}");
                writer.WriteLine($"yllcorner     {area.StartPointUTM.Northing:0}");
                writer.WriteLine($"cellsize      {area.CellSize}");
                writer.WriteLine($"NODATA_value  -9999");
                for (int y = 0; y < area.Size; y++)
                {
                    report.ReportItemsDone(y);
                    for (int x = 0; x < area.Size; x++)
                    {
                        writer.Write(elevationMatrix[x, area.Size - y - 1].ToString("0.00", CultureInfo.InvariantCulture));
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                }
            }
            report.TaskDone();
        }

        public void SavePreviewToPng(string path)
        {
            var min = 4000d;
            var max = 0d;

            for (int y = 0; y < area.Size; y++)
            {
                for (int x = 0; x < area.Size; x++)
                {
                    max = Math.Max(elevationMatrix[x, y], max);
                    min = Math.Min(elevationMatrix[x, y], min);
                }
            }

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
                img.Save(path);
            }
        }
    }
}
