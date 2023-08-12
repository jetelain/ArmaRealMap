using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.Services
{
    internal class Arma3Backup : IArma3Backup
    {
        public Arma3Backup(string backupZipFile, DateTime timestamp, DateTime dateTime, int revision)
        {
            BackupZipFile = backupZipFile;
            Timestamp = timestamp;
            DateTime = dateTime;
            Revision = revision;
        }

        public string BackupZipFile { get; }

        public DateTime Timestamp { get; }

        public DateTime DateTime { get; }

        public int Revision { get; }
    }
}