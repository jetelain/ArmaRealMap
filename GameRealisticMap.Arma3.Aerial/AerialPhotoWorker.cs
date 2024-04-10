using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Aerial
{
    internal class AerialPhotoWorker
    {
        private readonly IProgressSystem progress;

        private const int Resolution = 4;

        private ScreenshotWorker? screenshotWorker = null;

        private static Regex CaptureRegex = new Regex(@"x1=([\-0-9\.]+);y1=([\-0-9\.]+);x2=([\-0-9\.]+);y2=([\-0-9\.]+);model=(.+)");

        private int imageId = 1;

        private IProgressInteger? initial;

        private int imagesToProcess = 0;
        private IProgressInteger? images;

        internal AerialPhotoWorker(IProgressSystem progress)
        {
            this.progress = progress;
        }

        private void Interpret(string line, Process process)
        {
            if (images != null)
            {
                progress.WriteLine(line);
            }

            if (line.Contains("GRM::HELLO") && screenshotWorker == null)
            {
                screenshotWorker = ScreenshotWorker.FromProcess(process);
                initial?.Dispose();
                images = progress.CreateStep("Photos", imagesToProcess);
            }
            if (line.Contains("GRM::CLR"))
            {
                screenshotWorker?.CaptureTo("clear.png");
            }
            if (line.Contains("GRM::CAP"))
            {
                var match = CaptureRegex.Match(line);
                if (match.Success)
                {
                    var tempCapture = $"capture{imageId}.png";
                    screenshotWorker?.CaptureTo(tempCapture);

                    var tempClear = $"clear{imageId}.png";
                    File.Move("clear.png", tempClear, true);

                    imageId++;

                    Task.Run(() => ProcessImage(tempCapture, tempClear, match));
                }
            }
        }

        private void ProcessImage(string tempCapture, string tempClear, Match match)
        {
            var sw = Stopwatch.StartNew();
            var x1 = double.Parse(match.Groups[1].Value, NumberFormatInfo.InvariantInfo);
            var y1 = double.Parse(match.Groups[2].Value, NumberFormatInfo.InvariantInfo);
            var x2 = double.Parse(match.Groups[3].Value, NumberFormatInfo.InvariantInfo);
            var y2 = double.Parse(match.Groups[4].Value, NumberFormatInfo.InvariantInfo);
            var model = match.Groups[5].Value;
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
            imgSource.SaveAsPng(Path.GetFileNameWithoutExtension(model) + ".png");
            sw.Stop();
            progress.WriteLine($"{model} => {sw.ElapsedMilliseconds}");
            images?.ReportOneDone();
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

        public async Task TakePhotos(List<AerialModelRefence> models)
        {
            imagesToProcess = models.Count;

            initial = progress.CreateStep("Init",1);

            var workspace = Path.Combine(Path.GetTempPath(), "grm-a3");

            UnpackFiles(workspace, models);

            // TODO: Mods support

            var process = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                Arguments = @$"-window -noSplash -noPause -profiles=""{workspace}"" -cfg=""{workspace}\arma3.cfg"" -name=grm -autotest={workspace}\autotest.cfg",
                FileName = Path.Combine(Arma3ToolsHelper.GetArma3Path(), "Arma3_x64.exe")
            });

            if (process == null)
            {
                throw new ApplicationException("Unable to launch Arma 3");
            }

            Thread.Sleep(5000);

            var file = Directory.GetFiles(workspace, "*.rpt").OrderByDescending(f => File.GetLastWriteTimeUtc(f)).FirstOrDefault();

            if (file == null)
            {
                throw new ApplicationException("No log file found");
            }

            var cts = new CancellationTokenSource();

            var task = Task.Run(() => FileContentWatcher.Watch(file, cts.Token, line => Interpret(line, process)));

            process.WaitForExit();

            cts.Cancel();

            try
            {
                await task;
            }
            catch (TaskCanceledException)
            {

            }

            images?.Dispose();
        }

        private void UnpackFiles(string workspace, List<AerialModelRefence> models)
        {
            var ns = typeof(AerialPhotoWorker).Namespace;

            // Mission
            var mission = Path.Combine(workspace, "Aerial.VR");
            Directory.CreateDirectory(mission);
            File.WriteAllText(Path.Combine(mission, "data.sqf"), GetData(models));
            File.WriteAllText(Path.Combine(mission, "go.sqf"), GetEmbedded(ns + ".data.Aerial.VR.go.sqf"));
            File.WriteAllText(Path.Combine(mission, "init.sqf"), GetEmbedded(ns + ".data.Aerial.VR.init.sqf"));
            File.WriteAllText(Path.Combine(mission, "mission.sqm"), GetEmbedded(ns + ".data.Aerial.VR.mission.sqm"));
            
            // User profile (for graphics settings)
            var profile = Path.Combine(workspace, "Users", "grm");
            Directory.CreateDirectory(profile);
            File.WriteAllText(Path.Combine(profile, "grm.Arma3Profile"), GetEmbedded(ns + ".data.grm.Arma3Profile"));

            // Game config (for video settings)
            File.WriteAllText(Path.Combine(workspace, "arma3.cfg"), GetEmbedded(ns + ".data.arma3.cfg"));

            // Auto test file for automatic startup and close
            File.WriteAllText(Path.Combine(workspace, "autotest.cfg"), @$"class TestMissions
{{
	class TestCase01
	{{
		campaign = """";
		mission = ""{mission}""; 
	}};
}};");
        }

        private string? GetData(List<AerialModelRefence> models)
        {
            var sb = new StringBuilder();
            sb.AppendLine("[");
            var isFirst  = true;
            foreach(var model in models)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    sb.Append(",");
                }
                var altitude = Math.Max(Math.Ceiling((model.BboxMax.Y - model.BboxMin.Y) * 5), 100);
                var halfWidth = Math.Ceiling(Math.Max(model.BboxMax.Z - model.BboxMin.Z, model.BboxMax.X - model.BboxMin.X) * 2 / 3) + 5;
                sb.Append(FormattableString.Invariant($"['{model.Path}', {altitude}, {halfWidth}, {model.BoundingCenter.Y}]"));
                sb.AppendLine();
            }
            sb.AppendLine("]");
            return sb.ToString();
        }

        private static string? GetEmbedded(string v)
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream(v);
            return new StreamReader(stream!).ReadToEnd();
        }
    }
}
