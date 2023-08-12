using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.Services
{
    [Export(typeof(IArma3BackupService))]
    internal class Arma3BackupService : IArma3BackupService
    {
        private static readonly Regex FileNameRegex = new Regex("^([0-9]+)_([0-9]+)_([0-9]+).zip$", RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

        public int MaxBackup { get; set; } = 10;

        public void CreateBackup(string wrpFilePath, int revision, params string[] additionalFiles)
        {
            var timestamp = File.GetLastWriteTimeUtc(wrpFilePath);
            var backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameRealisticMap", "Arma3", "Backups", Path.GetFileNameWithoutExtension(wrpFilePath));
            if (Directory.Exists(backupDir))
            {
                LimitBackups(backupDir, MaxBackup - 1);
            }
            Directory.CreateDirectory(backupDir);
            var backupFile = Path.Combine(backupDir, FormattableString.Invariant($"{timestamp:yyyyMMddHHmmss}_{DateTime.UtcNow:yyyyMMddHHmmss}_{revision}.zip"));
            using var archive = new ZipArchive(File.Create(backupFile), ZipArchiveMode.Create);
            AddIfExists(archive, wrpFilePath);
            foreach(var file in additionalFiles)
            {
                AddIfExists(archive, file);
            }
        }

        private void LimitBackups(string backupDir, int maxCount)
        {
            var files = Directory.GetFiles(backupDir, "*.zip").OrderBy(f => File.GetLastWriteTimeUtc(f)).ToList();
            while (files.Count > maxCount)
            {
                File.Delete(files[0]);
                files.RemoveAt(0);
            }
        }

        private void AddIfExists(ZipArchive archive, string path)
        {
            if (File.Exists(path))
            {
                archive.CreateEntryFromFile(path, Path.GetFileName(path));
            }
        }

        public IReadOnlyCollection<IArma3Backup> GetBackups(string wrpFilePath)
        {
            var backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameRealisticMap", "Arma3", "Backups", Path.GetFileNameWithoutExtension(wrpFilePath));
            var result = new List<IArma3Backup>();
            if (Directory.Exists(backupDir))
            {
                foreach (var backupZipFile in Directory.GetFiles(backupDir, "*.zip").OrderByDescending(f => File.GetLastWriteTimeUtc(f)).ToList())
                {
                    var match = FileNameRegex.Match(Path.GetFileName(backupZipFile));
                    if (match.Success
                        && DateTime.TryParseExact(match.Groups[1].Value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)
                        && int.TryParse(match.Groups[3].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var revision))
                    {
                        result.Add(new Arma3Backup(backupZipFile, timestamp, File.GetLastWriteTimeUtc(backupZipFile), revision));
                    }
                }
            }
            return result;
        }
    }
}
