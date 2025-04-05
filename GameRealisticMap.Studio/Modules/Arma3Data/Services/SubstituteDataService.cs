using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Studio.Modules.Arma3Data.Services
{
    [Export(typeof(ISubstituteDataService))]
    internal class SubstituteDataService : ISubstituteDataService
    {
        private readonly IProgressTool progressTool;
        private readonly IArma3DataModule arma3DataModule;

        [ImportingConstructor]
        public SubstituteDataService(IProgressTool progressTool, IArma3DataModule arma3DataModule)
        {
            this.progressTool = progressTool;
            this.arma3DataModule = arma3DataModule;
        }

        public Task EnsureDataInstalled(IEnumerable<ModInfo> mods)
        {
            var missing = new List<ModInfo>();

            foreach (var mod in mods)
            {
                if (mod.IsCdlc)
                {
                    var cdlcDirectory = arma3DataModule.ProjectDrive.GetFullPath(mod.Prefix!);
                    if (!Directory.Exists(cdlcDirectory) || Directory.GetFiles(cdlcDirectory, "*.p3d", SearchOption.AllDirectories).Length == 0)
                    {
                        missing.Add(mod);
                    }   
                }
            }

            if (missing.Count == 0)
            {
                return Task.CompletedTask;
            }

            _ = progressTool.RunTask(Labels.InstallSubstituteFiles, async (progress) =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");

                foreach (var mod in missing)
                {
                    var cdlcDirectory = arma3DataModule.ProjectDrive.GetFullPath(mod.Prefix!);
                    var prefix = mod.Prefix! + "\\";
                    var temp = Path.Combine(Path.GetTempPath(), $"grm_cdlc_{mod.Prefix}.zip");
                    using var scope = progress.Scope.CreateScope(mod.Name);

                    await scope.Track("Download", async () => {
                        scope.WriteLine($"Downloading '{mod.DownloadUri}'...");
                        using var stream = await client.GetStreamAsync(mod.DownloadUri!);
                        using var fileStream = File.Create(temp);
                        await stream.CopyToAsync(fileStream);
                    });

                    scope.Track("Unzip", () => {
                        using var fileStream = File.OpenRead(temp);
                        using var zip = new ZipArchive(fileStream);
                        foreach (var entry in zip.Entries)
                        {
                            if (entry.FullName.EndsWith("/"))
                            {
                                continue;
                            }
                            var target = Path.Combine(cdlcDirectory, CleanUpPath(entry.FullName.Replace('/', '\\'), prefix));
                            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                            entry.ExtractToFile(target, true);
                        }
                    });

                    File.Delete(temp);
                }
            }, false);

            return Task.CompletedTask;
        }

        private static string CleanUpPath(string fullName, string prefix)
        {
            if (fullName.StartsWith(prefix))
            {
                return fullName.Substring(prefix.Length);
            }
            if ( fullName.StartsWith("SPE Substitute TB Files\\"))
            { 
                return fullName.Substring(24); 
            }
            return fullName;
        }
    }
}
