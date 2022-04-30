using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ArmaRealMap
{
    internal class Arma3ToolsHelper
    {
        private static readonly bool isLinux = Environment.OSVersion.Platform == PlatformID.Unix;

        public static void EnsureProjectDrive()
        {
            if (!isLinux && !Directory.Exists("P:\\"))
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

#pragma warning disable CA1416 // Valider la compatibilité de la plateforme
        internal static string GetArma3ToolsPath()
        {
            string path = "";
            using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 233800"))
            {
                if (key != null)
                {
                    path = (key.GetValue("InstallLocation") as string) ?? path;
                }
            }
            return path;
        }
#pragma warning restore CA1416 // Valider la compatibilité de la plateforme

        internal static void ImageToPAA(int num, Func<int, string> pattern)
        {
            ImageToPAA(Enumerable.Range(0, num).Select(pattern).ToList());
        }

        internal static void ImageToPAA(List<string> pattern, int? maxDegreeOfParallelism = null )
        {
            if (isLinux)
            {
                return; // ImageToPAA.exe does not works with wine
            }
            string imageToPaaExe = Path.Combine(GetArma3ToolsPath(), "ImageToPAA", "ImageToPAA.exe");
            var report = new ProgressReport("Png->PAA", pattern.Count);
            Parallel.ForEach(pattern, new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism ?? (Environment.ProcessorCount * 5 / 8) }, x =>
            {
                var proc = Process.Start(new ProcessStartInfo()
                {
                    FileName = imageToPaaExe,
                    RedirectStandardOutput = true,
                    Arguments = "\"" + x + "\"",
                });
                proc.OutputDataReceived += (_, e) => Trace.WriteLine(e.Data);
                proc.BeginOutputReadLine();
                proc.WaitForExit();
                report.ReportOneDone();
            });
            report.TaskDone();
        }
    }
}
