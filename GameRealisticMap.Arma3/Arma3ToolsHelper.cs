using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using GameRealisticMap.Reporting;
using Microsoft.Win32;

namespace GameRealisticMap.Arma3
{
    public static class Arma3ToolsHelper
    {
        public static void EnsureProjectDrive()
        {
            if (OperatingSystem.IsWindows() && !Directory.Exists("P:\\"))
            {
                Console.WriteLine("Mount project drive");
                string path = GetArma3ToolsPath();
                if (!string.IsNullOrEmpty(path))
                {
                    var processs = Process.Start(Path.Combine(path, @"WorkDrive\WorkDrive.exe"), "/Mount /y");
                    processs.WaitForExit();
                }
            }
        }

        [SupportedOSPlatform("windows")]
        internal static string GetSteamAppLocation(int id)
        {
            string path = "";
            using (var key = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {id}"))
            {
                if (key != null)
                {
                    path = (key.GetValue("InstallLocation") as string) ?? path;
                }
            }
            return path;
        }

        [SupportedOSPlatform("windows")]
        public static string GetArma3ToolsPath()
        {
            return GetSteamAppLocation(233800);
        }

        [SupportedOSPlatform("windows")]
        public static string GetArma3Path()
        {
            return GetSteamAppLocation(107410);
        }

        internal static async Task ImageToPAA(IProgressSystem system, IReadOnlyCollection<string> paths, int? maxDegreeOfParallelism = null)
        {
            if (!OperatingSystem.IsWindows())
            {
                return; // ImageToPAA.exe does not works with wine
            }
            string imageToPaaExe = Path.Combine(GetArma3ToolsPath(), "ImageToPAA", "ImageToPAA.exe");
            using var report = system.CreateStep("Png->PAA", paths.Count);
            var options = new ParallelOptions() { 
                MaxDegreeOfParallelism = maxDegreeOfParallelism ?? (Environment.ProcessorCount * 5 / 8) // ImageToPAA is very CPU aggressive
            };
            await Parallel.ForEachAsync(paths, options, async (x, _) =>
            {
                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = imageToPaaExe,
                    RedirectStandardOutput = true,
                    Arguments = "\"" + x + "\"",
                })!;
                proc.OutputDataReceived += (_, e) => Trace.WriteLine(e.Data);
                proc.BeginOutputReadLine();
                await proc.WaitForExitAsync().ConfigureAwait(false);
                report.ReportOneDone();
            }).ConfigureAwait(false);
        }

        [SupportedOSPlatform("windows")]
        public static async Task<int> BuildWithMikeroPboProject(string pboPrefix, string targetMod)
        {
            EnsureProjectDrive();
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = @"C:\Program Files (x86)\Mikero\DePboTools\bin\pboProject.exe", // Hard-coded because tool can only work from this location
                Arguments = @$"-R -E=""arma3"" -W -P -M=""{targetMod}"" ""P:\{pboPrefix}"""
            }) ;
            if (process == null)
            {
                return -1;
            }
            await process.WaitForExitAsync();
            return process.ExitCode;
        }

        public static string GetProjectDrivePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Arma 3 Projects");
        }
    }
}
