using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Microsoft.Win32;
using NLog.Targets;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandHandler]
    public class ExportLogsCommandHandler : CommandHandlerBase<ExportLogsCommandDefinition>
    {
        public override Task Run(Command command)
        {
            var dialog = new SaveFileDialog() { DefaultExt = ".zip", Filter = "ZIP Files|*.zip", FileName = $"GameRealisticMap-{DateTime.UtcNow:yyyyMMddHHmmss}.zip", Title = Labels.GenerateAnErrorReport };
            if (dialog.ShowDialog() == true)
            {
                NLog.LogManager.Flush();

                ChangeNLogKeepFileOpen(false); // NLog will close file

                CreateReport(dialog.FileName);

                ShellHelper.OpenUri(Path.GetDirectoryName(dialog.FileName) ?? string.Empty);

                ChangeNLogKeepFileOpen(false); // NLog will reopen file (much more performant)
            }
            return Task.CompletedTask;
        }

        private static void ChangeNLogKeepFileOpen(bool keepFileOpen)
        {
            foreach (var ftarget in NLog.LogManager.Configuration.AllTargets.OfType<FileTarget>())
            {
                ftarget.KeepFileOpen = keepFileOpen;
            }
        }

        private void CreateReport(string filename)
        {
            using var archive = new ZipArchive(File.Create(filename), ZipArchiveMode.Create);
            foreach (var logFile in Directory.GetFiles(App.GetLogsPath()))
            {
                archive.CreateEntryFromFile(logFile, Path.GetFileName(logFile));
            }
            var shell = IoC.Get<IShell>();
            foreach (var doc in shell.Documents.OfType<IMainDocument>())
            {
                var zipArchiveEntry = archive.CreateEntry(doc.ContentId + "-" + doc.FileName, CompressionLevel.Fastest);
                using (var zipStream = zipArchiveEntry.Open())
                {
                    try
                    {
                        doc.SaveTo(zipStream);
                    }
                    catch (Exception ex)
                    {
                        using var writer = new StreamWriter(zipStream);
                        writer.Write(ex.ToString());
                    }
                }
            }
        }
    }
}
