using System.Diagnostics;
using System.Globalization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Aerial
{
    internal class AerialPhotoWorker
    {
        private readonly string targetDirectory;
        private readonly string logFile;
        private readonly string workspace;
        private readonly ScreenshotWorker screenshotWorker;

        private const int Resolution = 4;

        private int imageId = 1;

        internal AerialPhotoWorker()
        {
            this.screenshotWorker = new ScreenshotWorker(Process.GetCurrentProcess().MainWindowHandle);
            this.workspace = Path.Combine(Path.GetTempPath(), "grm-a3");
            this.targetDirectory = Path.Combine(workspace, "result");
            this.logFile = Path.Combine(workspace, "log.txt");
            File.WriteAllText(logFile, "");
        }

        public void TakeClear()
        {
            screenshotWorker.CaptureTo(Path.Combine(workspace, "clear.bmp"));
        }

        public void TakeImage(string[] data)
        {
            var tempCapture = Path.Combine(workspace, $"img{imageId}-2cap.bmp");
            screenshotWorker.CaptureTo(tempCapture);

            var tempClear = Path.Combine(workspace, $"img{imageId}-1clr.bmp");
            File.Move(Path.Combine(workspace, "clear.bmp"), tempClear, true);

            imageId++;

            Task.Run(() => ProcessImage(tempCapture, tempClear, data));
        }

        private void ProcessImage(string tempCapture, string tempClear, string[] data)
        {
            try
            {
                var x1 = double.Parse(data[0], NumberFormatInfo.InvariantInfo);
                var y1 = double.Parse(data[1], NumberFormatInfo.InvariantInfo);
                var x2 = double.Parse(data[2], NumberFormatInfo.InvariantInfo);
                var y2 = double.Parse(data[3], NumberFormatInfo.InvariantInfo);
                var model = data[4].Trim('"');
                var widthInMeters = Math.Abs(x1 - x2);
                var heightInMeters = Math.Abs(y1 - y2);

                using var imgSource = Image.Load<Rgba32>(tempCapture);
                using (var imgClear = Image.Load<Rgba32>(tempClear))
                {
                    MakeClearTransparent(imgSource, imgClear);
                }
                MakeAlphaChannelSmooth(imgSource);
                File.Delete(tempCapture);
                File.Delete(tempClear);
                imgSource.Mutate(p => p.Resize((int)Math.Round(widthInMeters * Resolution), (int)Math.Round(heightInMeters * Resolution)));

                var targetFile = Path.Combine(targetDirectory, Path.ChangeExtension(model, ".png"));
                var targetDir = Path.GetDirectoryName(targetFile);
                if (!string.IsNullOrEmpty(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }
                imgSource.Mutate(m => m.RotateFlip(RotateMode.Rotate180, FlipMode.None));
                imgSource.SaveAsPng(targetFile);
            }
            catch (Exception ex)
            {
                AppendToLog(ex.ToString());
            }
        }

        private void MakeClearTransparent(Image<Rgba32> imgSource, Image<Rgba32> imgClear)
        {
            for (var x = 0; x < imgSource.Width; x++)
            {
                for (var y = 0; y < imgSource.Height; y++)
                {
                    if (x < 64 && y < 24)
                    {
                        // FPS Counter
                        imgSource[x, y] = new Rgba32(0, 0, 0, 0);
                    }
                    else
                    {
                        var clr = imgClear[x, y];
                        var src = imgSource[x, y];
                        if (clr == src || (clr.ToVector4() - src.ToVector4()).LengthSquared() < 0.0001)
                        {
                            imgSource[x, y] = new Rgba32(0, 0, 0, 0);
                        }
                    }
                }
            }
        }

        private void MakeAlphaChannelSmooth(Image<Rgba32> imgSource)
        {
            for (var x = 1; x < imgSource.Width - 1; x++)
            {
                for (var y = 1; y < imgSource.Height - 1; y++)
                {
                    if (imgSource[x, y].A == 255 && (
                          imgSource[x - 1, y - 1].A == 0 ||
                          imgSource[x - 1, y].A == 0 ||
                          imgSource[x - 1, y + 1].A == 0 ||
                          imgSource[x, y - 1].A == 0 ||
                          imgSource[x, y + 1].A == 0 ||
                          imgSource[x + 1, y - 1].A == 0 ||
                          imgSource[x + 1, y].A == 0 ||
                          imgSource[x + 1, y + 1].A == 0))
                    {
                        var px = imgSource[x, y];
                        px.A = 128;
                        imgSource[x, y] = px;
                    }
                }
            }
        }


        public void AppendToLog(string message)
        {
            File.AppendAllText(logFile, message + "\r\n\r\n");
        }
    }
}
