using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Aerial
{
    internal class Program
    {
        private const int Resolution = 4;

        private static ScreenshotWorker? screenshotWorker = null;

        private static Regex CaptureRegex = new Regex(@"x1=([\-0-9\.]+);y1=([\-0-9\.]+);x2=([\-0-9\.]+);y2=([\-0-9\.]+);model=(.+)");

        private static int imageId = 1;

        private static void Interpret(string line, Process process)
        {
            //Console.WriteLine(line);
            if (line.Contains("GRM::HELLO") && screenshotWorker == null)
            {
                screenshotWorker = ScreenshotWorker.FromProcess(process);
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

        private static void ProcessImage(string tempCapture, string tempClear, Match match)
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
            imgSource.Mutate(p => p.Resize((int)Math.Round(widthInMeters* Resolution), (int)Math.Round(heightInMeters* Resolution)));
            imgSource.SaveAsPng(Path.GetFileNameWithoutExtension(model) + ".png");
            sw.Stop();
            Console.WriteLine($"{model} => {sw.ElapsedMilliseconds}");


        }

        private static void MakeClearTransparent(Image<Rgba32> imgSource, Image<Rgba32> imgClear)
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

        private static void MakeAlphaChannelSmooth(Image<Rgba32> imgSource)
        {
            for (var x = 1; x < imgSource.Width-1; x++)
            {
                for (var y = 1; y < imgSource.Height-1; y++)
                {
                    if (imgSource[x, y].A == 255 && (
                          imgSource[x - 1, y - 1].A == 0 ||
                          imgSource[x - 1, y].A == 0 ||
                          imgSource[x - 1, y + 1].A == 0 ||
                          imgSource[x, y - 1].A == 0 ||
                          imgSource[x, y + 1].A == 0 ||
                          imgSource[x + 1, y - 1].A == 0 ||
                          imgSource[x + 1, y].A == 0 ||
                          imgSource[x + 1, y + 1].A == 0 ))
                    {
                        var px = imgSource[x, y];
                        px.A = 128;
                        imgSource[x, y] = px;
                    }
                }
            }
        }

        static async Task Main(string[] args)
        {
            Native.SetProcessDPIAware();

            var workspace = @"C:\temp\arma3"; // FIXME

            UnpackFiles(workspace);

            var process = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                Arguments = @$"-window -noSplash -noPause -profiles=""{workspace}"" -cfg=""{workspace}\arma3.cfg"" -name=grm -autotest={workspace}\autotest.cfg",
                FileName = @"C:\Program Files (x86)\Steam\steamapps\common\Arma 3\arma3.exe" // FIXME
            });

            if (process == null)
            {
                Console.WriteLine("Unable to launch Arma 3");
                return;
            }

            Thread.Sleep(5000);

            var file = Directory.GetFiles(workspace, "*.rpt").OrderByDescending(f => File.GetLastWriteTimeUtc(f)).FirstOrDefault();

            if (file == null)
            {
                Console.WriteLine("No log file found");
                return;
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
        }

        private static void UnpackFiles(string workspace)
        {
            var mission = Path.Combine(workspace, "Aerial.VR");
            Directory.CreateDirectory(mission);

            File.WriteAllText(Path.Combine(mission, "data.sqf"), @"[
	['a3\structures_f\dominants\wip\wip_f.p3d', 300, 30],
	['a3\plants_f\tree\t_PinusS2s_F.p3d', 100, 5]
]"); // FIXME: This should be generated

            File.WriteAllText(Path.Combine(mission, "go.sqf"), GetEmbedded("GameRealisticMap.Arma3.Aerial.data.Aerial.VR.go.sqf"));
            File.WriteAllText(Path.Combine(mission, "init.sqf"), GetEmbedded("GameRealisticMap.Arma3.Aerial.data.Aerial.VR.init.sqf"));
            File.WriteAllText(Path.Combine(mission, "mission.sqm"), GetEmbedded("GameRealisticMap.Arma3.Aerial.data.Aerial.VR.mission.sqm"));

            var profile = Path.Combine(workspace, "Users", "grm");
            Directory.CreateDirectory(profile);
            File.WriteAllText(Path.Combine(profile, "grm.Arma3Profile"), GetEmbedded("GameRealisticMap.Arma3.Aerial.data.grm.Arma3Profile"));

            File.WriteAllText(Path.Combine(workspace, "arma3.cfg"), GetEmbedded("GameRealisticMap.Arma3.Aerial.data.arma3.cfg"));

            File.WriteAllText(Path.Combine(workspace, "autotest.cfg"), @$"class TestMissions
{{
	class TestCase01
	{{
		campaign = """";
		mission = ""{mission}""; 
	}};
}};");
        }

        private static string? GetEmbedded(string v)
        {
            using var stream = typeof(Program).Assembly.GetManifestResourceStream(v);
            return new StreamReader(stream!).ReadToEnd();
        }
    }
}
