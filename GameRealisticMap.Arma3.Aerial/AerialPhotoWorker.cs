using System.Diagnostics;
using System.Text;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Aerial
{
    internal class AerialPhotoWorker
    {
        private readonly IProgressSystem progress;
        private readonly List<AerialModelRefence> models;
        private readonly string targetDirectory;
        private readonly string workspace;
        private IProgressInteger? initial;
        private IProgressInteger? images;

        internal AerialPhotoWorker(IProgressSystem progress, List<AerialModelRefence> models, string targetDirectory)
        {
            this.progress = progress;
            this.models = models;
            this.targetDirectory = targetDirectory;
            this.workspace = Path.Combine(Path.GetTempPath(), "grm-a3");
        }

        private void Interpret(string line, Process process)
        {
            progress.WriteLine(line);

            if (line.Contains("GRM::HELLO"))
            {
                initial?.Dispose();
                images = progress.CreateStep("Photos", models.Count);
            }

            if (line.Contains("GRM::ONE"))
            {
                images?.ReportOneDone();
            }
        }


        public async Task TakePhotos()
        {
            initial = progress.CreateStep("Init",1);

            UnpackFiles();

            // TODO: Mods support

            var process = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                Arguments = @$"-window -noSplash -noPause -profiles=""{workspace}"" -cfg=""{workspace}\arma3.cfg"" -name=grm -autotest={workspace}\autotest.cfg -mod=C:\temp\arma3\@aerial",
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

        private void UnpackFiles()
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
