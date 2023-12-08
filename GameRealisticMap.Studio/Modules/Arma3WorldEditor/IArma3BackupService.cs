using System.Collections.Generic;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor
{
    internal interface IArma3BackupService
    {
        void CreateBackup(string wrpFilePath, int revision, IEnumerable<string> additionalFiles);

        IReadOnlyCollection<IArma3Backup> GetBackups(string wrpFilePath);
    }
}
