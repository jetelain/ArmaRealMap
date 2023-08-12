using System;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    public interface IArma3Backup
    {
        string BackupZipFile { get; }

        DateTime Timestamp { get; }

        DateTime DateTime { get; }

        int Revision { get; }
    }
}