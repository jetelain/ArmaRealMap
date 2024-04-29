using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Aerial
{
    [SupportedOSPlatform("windows")]
    public sealed class AerialPhotoWorker
    {
        private readonly IProgressSystem progress;
        private readonly List<AerialModelRefence> models;
        private readonly string targetDirectory;
        private readonly string workspace;
        private readonly List<ModDependencyDefinition> dependencies;
        private IProgressInteger? initial;
        private IProgressInteger? images;

        public AerialPhotoWorker(IProgressSystem progress, List<AerialModelRefence> models, string targetDirectory, IEnumerable<ModDependencyDefinition> dependencies)
        {
            this.progress = progress;
            this.models = models;
            this.targetDirectory = targetDirectory;
            this.workspace = Path.Combine(Path.GetTempPath(), "grm-a3");
            this.dependencies = dependencies.ToList();
        }

        private void ProcessRptLine(string line)
        {
            progress.WriteLine(line);

            if (line.Contains("GRM::HELLO"))
            {
                initial?.Dispose();
                initial = null;
                images = progress.CreateStep("Photos", models.Count);
            }

            if (line.Contains("GRM::ONE"))
            {
                images?.ReportOneDone();
            }
        }


        public async Task TakePhotos()
        {
            UnpackFiles();

            initial = progress.CreateStep("Init", 1);

            var process = StartArma3();

            await Task.Delay(5000); // Wait for Arma to create the .rpt file

            var rptFilePath = SearchRptFile();

            var cts = new CancellationTokenSource();

            _ = Task.Run(() => FileContentWatcher.Watch(rptFilePath, cts.Token, ProcessRptLine));

            await process.WaitForExitAsync();

            cts.Cancel();

            initial?.Dispose();
            images?.Dispose();

            var resultPath = Path.Combine(workspace, "result");
            var resultImages = Directory.GetFiles(Path.Combine(workspace, "result"), "*.png", SearchOption.AllDirectories);
            foreach(var pngFilePath in resultImages.ProgressStep(progress, "Copy"))
            {
                var relPath = pngFilePath.Substring(resultPath.Length + 1);
                var targetPath = Path.Combine(targetDirectory, relPath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(pngFilePath, targetPath, true);
            }
        }

        private string SearchRptFile()
        {
            var file = Directory.GetFiles(workspace, "*.rpt").OrderByDescending(f => File.GetLastWriteTimeUtc(f)).FirstOrDefault();
            if (file == null)
            {
                throw new ApplicationException("No log file found");
            }
            return file;
        }

        private Process StartArma3()
        {
            // TODO: Pack extension in mod

            var workshop = Arma3ToolsHelper.GetArma3WorkshopPath();
            var mods = string.Join(";", dependencies.Select(d => Path.Combine(workshop, d.SteamId)).Concat(new[] { Path.Combine(workshop, "3016661145") }));

            var process = Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                Arguments = @$"-window -noSplash -noPause -profiles=""{workspace}"" -cfg=""{workspace}\arma3.cfg"" -name=grm -autotest={workspace}\autotest.cfg ""-mod={mods}""",
                FileName = Path.Combine(Arma3ToolsHelper.GetArma3Path(), "Arma3_x64.exe")
            });
            if (process == null)
            {
                throw new ApplicationException("Unable to launch Arma 3");
            }
            return process;
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
            using var stream = typeof(AerialPhotoWorker).Assembly.GetManifestResourceStream(v);
            return new StreamReader(stream!).ReadToEnd();
        }
    }
}
