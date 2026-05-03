using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    /// <summary>
    /// Represents a single versioned backup of an Arma 3 WRP terrain file.
    /// Backups are stored as ZIP archives and can be restored if a generation step
    /// produces an undesirable result.
    /// </summary>
    public interface IArma3Backup
    {
        string BackupZipFile { get; }

        DateTime Timestamp { get; }

        DateTime DateTime { get; }

        int Revision { get; }
    }
}