using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoordinateSharp;
using GameRealisticMap.ElevationModel;
using MapToolkit;
using MapToolkit.Databases;
using MapToolkit.DataCells;

namespace ArmaRealMap.TerrainData.ElevationModel
{
    public static class ElevationGridExtensions
    {
        public static void LoadFromSRTM(this ElevationGrid grid, MapInfos area, SRTMConfig configSRTM)
        {
            var db = new DemDatabase(new DemHttpStorage(configSRTM.CacheLocation, new Uri("https://dem.pmad.net/SRTM1/")));

            var startPointUTM = area.StartPointUTM;
            var eager = new EagerLoad(false);

            var done = 0;
            double delta = 1d / 3600d;

            var points = new[] { area.SouthWest, area.NorthEast, area.NorthWest, area.SouthEast };

            var view = db.CreateView<ushort>(
                new Coordinates(points.Min(p => p.Latitude.ToDouble()) - 0.001, points.Min(p => p.Longitude.ToDouble()) - 0.001),
                new Coordinates(points.Max(p => p.Latitude.ToDouble()) + 0.001, points.Max(p => p.Longitude.ToDouble()) + 0.001))
                .GetAwaiter()
                .GetResult()
                .ToDataCell();

            var report = new ProgressReport("LoadFromSRTM", area.Size);
            Parallel.For(0, area.Size, y =>
            {
                for (int x = 0; x < area.Size; x++)
                {
                    var latLong = area.TerrainToLatLong(x * area.CellSize, y * area.CellSize);
                    var elevation = GetElevationBilinear(view, latLong.Latitude.ToDouble(), latLong.Longitude.ToDouble());
                    if (area.CellSize > 30) // Smooth cells larger than SRTM resolution
                    {
                        elevation = (new[] { elevation,
                            GetElevationBilinear(view, latLong.Latitude.ToDouble() - delta, latLong.Longitude.ToDouble() - delta),
                            GetElevationBilinear(view, latLong.Latitude.ToDouble() - delta, latLong.Longitude.ToDouble() + delta),
                            GetElevationBilinear(view, latLong.Latitude.ToDouble() + delta, latLong.Longitude.ToDouble() - delta),
                            GetElevationBilinear(view, latLong.Latitude.ToDouble() + delta, latLong.Longitude.ToDouble() + delta)
                        }).Average();
                    }
                    grid[x, y] = (float)elevation;
                }
                report.ReportItemsDone(Interlocked.Increment(ref done));
            });

            report.TaskDone();
        }

        private static double GetElevationBilinear(DemDataCellBase<ushort> view, double lat, double lon)
        {
            return view.GetLocalElevation(new Coordinates(lat, lon), DefaultInterpolation.Instance);
        }

        public static void LoadFromAsc(this ElevationGrid grid, MapInfos area, string path)
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
                        grid[x, y] = (float)elevation;
                        x++;
                    }
                    report.ReportItemsDone(area.Size - y);
                    y--;
                }
            }
            report.TaskDone();
        }
        public static void LoadFromBin(this ElevationGrid grid, MapInfos area, string path)
        {
            var report = new ProgressReport("LoadFromBin", area.Size);
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                var size = reader.ReadInt32();
                if (size != area.Size)
                {
                    throw new IOException("File size does not match");
                }
                for (int y = 0; y < area.Size; y++)
                {
                    for (int x = 0; x < area.Size; x++)
                    {
                        grid[x, y] = reader.ReadSingle();
                    }
                    report.ReportItemsDone(y);
                }
            }
            report.TaskDone();
        }

        public static void SaveToBin(this ElevationGrid grid, MapInfos area, string path)
        {
            var report = new ProgressReport("SaveToBin", area.Size);
            using (var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(area.Size);
                for (int y = 0; y < area.Size; y++)
                {
                    for (int x = 0; x < area.Size; x++)
                    {
                        writer.Write(grid[x, y]);
                    }
                    report.ReportItemsDone(y);
                }
            }
            report.TaskDone();
        }

        public static void SaveToAsc(this ElevationGrid grid, MapInfos area, string path)
        {
            var report = new ProgressReport("SaveToAsc", area.Size);

            using (var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
            {
                SaveToAsc(grid, report, area, writer);
            }
            report.TaskDone();
        }

        public static void SaveToAsc(ElevationGrid grid, ProgressReport report, MapInfos area, TextWriter writer)
        {
            writer.WriteLine($"ncols         {area.Size}");
            writer.WriteLine($"nrows         {area.Size}");
            writer.WriteLine($"xllcorner     200000");
            writer.WriteLine($"yllcorner     0");
            writer.WriteLine($"cellsize      {area.CellSize}");
            writer.WriteLine($"NODATA_value  -9999");
            for (int y = 0; y < area.Size; y++)
            {
                report?.ReportItemsDone(y);
                for (int x = 0; x < area.Size; x++)
                {
                    writer.Write(grid[x, area.Size - y - 1].ToString("0.00", CultureInfo.InvariantCulture));
                    writer.Write(" ");
                }
                writer.WriteLine();
            }
        }

    }
}
