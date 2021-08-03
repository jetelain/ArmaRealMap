using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using CoordinateSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SRTM;
using SRTM.Sources.NASA;

namespace ArmaRealMap.ElevationModel
{
    public class ElevationGrid
    {
        private readonly MapInfos area;
        public readonly float[,] elevationGrid;

        private readonly Vector2 cellSize;
        private readonly Vector2 cellDelta;

        public ElevationGrid(MapInfos areaInfos)
        {
            area = areaInfos;
            elevationGrid = new float[area.Size, area.Size];
            cellSize = new Vector2(area.CellSize);
            cellDelta = new Vector2(0.5f); // Elevation is at the center of the cell
        }
        public ElevationGrid(ElevationGrid other)
        {
            area = other.area;
            elevationGrid = (float[,])other.elevationGrid.Clone();
            cellSize = other.cellSize;
            cellDelta = other.cellDelta;
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
                    elevationGrid[x, y] = (float)elevation;
                }
                report.ReportItemsDone(Interlocked.Increment(ref done));
            });

            report.TaskDone();
        }
        internal float ElevationAround(TerrainPoint p)
        {
            return ElevationAround(p, cellSize.X / 2);
        }

        internal float ElevationAround(TerrainPoint p, float radius)
        {
            return 
                (ElevationAt(p) + 
                ElevationAt(p + new Vector2(-radius, -radius)) +
                ElevationAt(p + new Vector2(radius, -radius)) +
                ElevationAt(p + new Vector2(-radius, radius)) +
                ElevationAt(p + new Vector2(radius, radius))) / 5f;
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
                        elevationGrid[x, y] = (float)elevation;
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
                writer.WriteLine($"xllcorner     200000");
                writer.WriteLine($"yllcorner     0");
                writer.WriteLine($"cellsize      {area.CellSize}");
                writer.WriteLine($"NODATA_value  -9999");
                for (int y = 0; y < area.Size; y++)
                {
                    report.ReportItemsDone(y);
                    for (int x = 0; x < area.Size; x++)
                    {
                        writer.Write(elevationGrid[x, area.Size - y - 1].ToString("0.00", CultureInfo.InvariantCulture));
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                }
            }
            report.TaskDone();
        }

        public void SavePreview(string path)
        {
            var min = 4000d;
            var max = -1000d;

            var report = new ProgressReport("ElevationPreview", area.Size * 2);

            for (int y = 0; y < area.Size; y++)
            {
                report.ReportItemsDone(y);
                for (int x = 0; x < area.Size; x++)
                {
                    max = Math.Max(elevationGrid[x, y], max);
                    min = Math.Min(elevationGrid[x, y], min);
                }
            }

            var legend = new[]
            {
                new { E = min, Color = Color.LightBlue.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.10, Color = Color.DarkGreen.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.15, Color = Color.Green.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.40, Color = Color.Yellow.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = min + (max - min) * 0.70, Color = Color.Red.ToPixel<Rgb24>().ToScaledVector4() },
                new { E = max, Color = Color.Maroon.ToPixel<Rgb24>().ToScaledVector4() }
            };
            using (var img = new Image<Rgb24>(area.Size, area.Size))
            {
                for (int y = 0; y < area.Size; y++)
                {
                    report.ReportItemsDone(area.Size + y);
                    for (int x = 0; x < area.Size; x++)
                    {
                        var elevation = elevationGrid[x, y];
                        var before = legend.Where(e => e.E <= elevation).Last();
                        var after = legend.FirstOrDefault(e => e.E > elevation) ?? legend.Last();
                        var scale = (float)((elevation - before.E) / (after.E - before.E));
                        Rgb24 rgb = new Rgb24();
                        rgb.FromScaledVector4(Vector4.Lerp(before.Color, after.Color, scale));
                        img[x, area.Size - y - 1] = rgb;
                    }
                }
                img.Save(path);
                report.TaskDone();
            }
        }

        private float ElevationAtCell(int x, int y)
        {
            return elevationGrid[
                Math.Min(Math.Max(0, x), area.Size - 1),
                Math.Min(Math.Max(0, y), area.Size - 1)];
        }

        public float ElevationAt(TerrainPoint point)
        {
            var pos = ToGrid(point);
            var x1 = (int)Math.Floor(pos.X);
            var y1 = (int)Math.Floor(pos.Y);
            var x2 = (int)Math.Ceiling(pos.X);
            var y2 = (int)Math.Ceiling(pos.Y);
            return Blerp(
                ElevationAtCell(x1, y1),
                ElevationAtCell(x2, y1),
                ElevationAtCell(x1, y2),
                ElevationAtCell(x2, y2),
                x2 - pos.X,
                y2 - pos.Y);
        }

        private float Lerp(float start, float end, float delta)
        {
            return start + (end - start) * delta;
        }

        private float Blerp(float val00, float val10, float val01, float val11, float deltaX, float deltaY)
        {
            return Lerp(Lerp(val11, val01, deltaX), Lerp(val10, val00, deltaX), deltaY);
        }

        public Vector2 ToGrid(TerrainPoint point)
        {
            return ((point.Vector - area.P1.Vector) / cellSize) - cellDelta;
        }

        public TerrainPoint ToTerrain(int x, int y)
        {
            return ToTerrain(new Vector2(x, y));
        }

        public TerrainPoint ToTerrain(Vector2 grid)
        {
            return new TerrainPoint(((grid + cellDelta) * cellSize)  + area.P1.Vector);
        }

        public ElevationGridArea PrepareToMutate(TerrainPoint min, TerrainPoint max)
        {
            var posMin = ToGrid(min);
            var posMax = ToGrid(max);
            var x1 = (int)Math.Floor(posMin.X);
            var y1 = (int)Math.Floor(posMin.Y);
            var x2 = (int)Math.Ceiling(posMax.X);
            var y2 = (int)Math.Ceiling(posMax.Y);

            float maxElevation = 0.0f;
            float minElevation = 4000.0f;

            for (int x = x1; x < x2; ++x)
            {
                for (int y = y1; y < y2; ++y)
                {
                    if (x >= 0 && y >= 0 && x < area.Size && y < area.Size)
                    {
                        var elevation = elevationGrid[x, y];
                        if ( elevation > maxElevation)
                        {
                            maxElevation = elevation;
                        }
                        if (elevation < minElevation)
                        {
                            minElevation = elevation;
                        }
                    }
                }
            }
            var delta = maxElevation - minElevation;
            var grid = new ElevationGridArea(this, x1, y1, x2 - x1 + 1, y2 - y1 + 1, minElevation, delta);
            /*for (int x = x1; x < x2; ++x)
            {
                for (int y = y1; y < y2; ++y)
                {
                    if (x >= 0 && y >= 0 && x < area.Size && y < area.Size)
                    {
                        var b = (byte)((elevationGrid[x, y] - minElevation) * (float)byte.MaxValue / delta);
                        grid.Image[x - x1, y - y1] = new Rgba32(b, b, b, byte.MaxValue);
                    }
                }
            }*/
            return grid;
        }

        public ElevationGridArea PrepareToMutate(TerrainPoint min, TerrainPoint max, float minElevation, float maxElevation)
        {
            var posMin = ToGrid(min);
            var posMax = ToGrid(max);
            var x1 = (int)Math.Floor(posMin.X);
            var y1 = (int)Math.Floor(posMin.Y);
            var x2 = (int)Math.Ceiling(posMax.X);
            var y2 = (int)Math.Ceiling(posMax.Y);
            var delta = maxElevation - minElevation;
            return new ElevationGridArea(this, x1, y1, x2 - x1 + 1, y2 - y1 + 1, minElevation, delta);
        }

        public void Apply (int startX, int startY, Image<Rgba32> data, float minElevation, float elevationDelta)
        {
            for(int x = 0; x < data.Width; ++x)
            {
                for (int y = 0; y < data.Height; ++y)
                {
                    if (x + startX >= 0 && y + startY >= 0 && x + startX < area.Size && y + startY < area.Size)
                    {
                        var pixel = data[x, y];
                        if (pixel.A != byte.MinValue)
                        {
                            var pixelElevation = minElevation + (elevationDelta * pixel.B / (float)byte.MaxValue);
                            if (pixel.A == byte.MaxValue)
                            {
                                elevationGrid[x + startX, y + startY] = pixelElevation;
                            }
                            else
                            {
                                var existingElevation = elevationGrid[x + startX, y + startY];
                                elevationGrid[x + startX, y + startY] = existingElevation + ((pixelElevation - existingElevation) * pixel.A / (float)byte.MaxValue);
                            }
                        }
                    }
                }
            }

        }
    }
}
