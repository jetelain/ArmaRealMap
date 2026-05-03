using System.Collections.Generic;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    /// <summary>
    /// Creates and lists versioned ZIP backups of Arma 3 WRP files and accompanying
    /// assets before each generation run, providing a rollback safety net.
    /// </summary>
    internal interface IArma3BackupService
    {
        void CreateBackup(string wrpFilePath, int revision, IEnumerable<string> additionalFiles);

        IReadOnlyCollection<IArma3Backup> GetBackups(string wrpFilePath);
    }
}
