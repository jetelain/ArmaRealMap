using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ArmaRealMap.TerrainData.GroundDetailTextures
{
    internal static class TerrainTileHelper
    {
        internal static void ImageToPAA(int num, Func<int, string> pattern)
        {
            string imageToPaaExe;
            string argumentPrefix;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                imageToPaaExe = "wine";
                var location = Environment.GetEnvironmentVariable("ARMA3_IMAGETOPAA") ?? "ImageToPAA.exe";
                if ( !File.Exists(location) )
                {
                    throw new Exception($"'{location}' was not found. Please set variable ARMA3_IMAGETOPAA to set location.");
                }
                argumentPrefix = location + " ";
            }
            else
            {
                imageToPaaExe = Path.Combine(Program.GetArma3ToolsPath(), "ImageToPAA", "ImageToPAA.exe");
                argumentPrefix = string.Empty;
            }

            var report = new ProgressReport("Png->PAA", num);
            Parallel.For(0, num, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, x =>
            {
                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = imageToPaaExe,
                    RedirectStandardOutput = true,
                    Arguments = argumentPrefix + pattern(x),
                });
                proc.OutputDataReceived += (_, e) => Trace.WriteLine(e.Data);
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                report.ReportOneDone();
            });
            report.TaskDone();
        }

        internal static void FillEdges(Image<Rgb24> realSat, int x, int num, Image<Rgb24> tile, int y, Point pos)
        {
            if (x == 0)
            {
                FillX(tile, pos.X, -1);
            }
            else if (x == num - 1)
            {
                FillX(tile, pos.X + realSat.Width - 1, +1);
            }
            if (y == 0)
            {
                FillY(tile, pos.Y, -1);
            }
            else if (y == num - 1)
            {
                FillY(tile, pos.Y + realSat.Height - 1, +1);
            }
        }

        private static void FillY(Image<Rgb24> tile, int sourceY, int d)
        {
            var y = sourceY + d;
            while (y >= 0 && y < tile.Height)
            {
                for (int x = 0; x < tile.Width; ++x)
                {
                    tile[x, y] = tile[x, sourceY];
                }
                y += d;
            }
        }

        private static void FillX(Image<Rgb24> tile, int sourceX, int d)
        {
            var x = sourceX + d;
            while (x >= 0 && x < tile.Width)
            {
                for (int y = 0; y < tile.Height; ++y)
                {
                    tile[x, y] = tile[sourceX, y];
                }
                x += d;
            }
        }

    }
}
