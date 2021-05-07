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
        private readonly MapInfos area;
        internal readonly Image<Elevation> elevationImage;

        internal ElevationGrid(MapInfos areaInfos)
        {
            area = areaInfos;
            elevationImage = new Image<Elevation>(area.Size, area.Size);
        }

        public void LoadFromSRTM(ConfigSRTM configSRTM)
        {
            var credentials = new NetworkCredential(configSRTM.Login, configSRTM.Password);
            var srtmData = new SRTMData(configSRTM.Cache, new NASASource(credentials));
            var startPointUTM = area.StartPointUTM;
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

                    var elevation = srtmData.GetElevationBilinear(latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble()) ?? double.NaN;
                    elevationImage[x, y] = new Elevation(elevation);
                }
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
                        var elevation = double.Parse(item, CultureInfo.InvariantCulture);
                        elevationImage[x, y] = new Elevation(elevation);
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
                        writer.Write(elevationImage[x, area.Size - y - 1].ToString());
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

            var report = new ProgressReport("SavePreviewToPng", area.Size * 2);

            for (int y = 0; y < area.Size; y++)
            {
                report.ReportItemsDone(y);
                for (int x = 0; x < area.Size; x++)
                {
                    max = Math.Max(elevationImage[x, y].Value, max);
                    min = Math.Min(elevationImage[x, y].Value, min);
                }
            }

            using (var img = new Image<Rgb24>(area.Size, area.Size))
            {
                for (int y = 0; y < area.Size; y++)
                {
                    report.ReportItemsDone(area.Size + y);
                    for (int x = 0; x < area.Size; x++)
                    {
                        byte value = (byte)((elevationImage[x, y].Value - min) * 255 / (max - min));
                        img[x, area.Size - y - 1] = new Rgb24(value, value, value);
                    }
                }
                img.Save(path);
                report.TaskDone();
            }
        }
    }
}
