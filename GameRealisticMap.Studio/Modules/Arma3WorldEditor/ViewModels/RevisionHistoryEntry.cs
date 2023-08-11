using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using BIS.Core.Streams;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class RevisionHistoryEntry : PropertyChangedBase
    {
        private readonly Arma3WorldEditorViewModel parent;
        private readonly IArma3Backup? backup;
        private bool _isActive;

        public RevisionHistoryEntry(Arma3WorldEditorViewModel parent)
        {
            this.parent = parent;
            _isActive = true;
            Revision = parent.ConfigFile?.Revision ?? 0;
            Date = File.GetLastWriteTime(parent.FilePath).ToString(CultureInfo.InstalledUICulture);
        }

        public RevisionHistoryEntry(Arma3WorldEditorViewModel parent, IArma3Backup backup)
        {
            this.parent = parent;
            this.backup = backup;
            Revision = backup.Revision;
            Date = backup.Timestamp.ToLocalTime().ToString(CultureInfo.InstalledUICulture);
        }

        public int Revision { get; }

        public string Date { get; }

        public bool IsActive 
        { 
            get { return _isActive; } 
            set { if (_isActive != value) { _isActive = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(IsNotActive)); } } 
        }

        public bool IsNotActive => !_isActive;

        public async Task Load()
        {
            if (parent.IsDirty && !parent.Backups.Any(b => b.IsActive))
            {
                if (MessageBox.Show("Restoring a previous version causes the loss of current modifications, do you want to continue?", string.Format("Restore {0}", Date), MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            if (backup == null)
            {
                await parent.Load(parent.FilePath); // Will reset everything
                return;
            }

            if (await TryLoadFromArchive(backup.BackupZipFile))
            {
                parent.ClearActive();
                parent.IsDirty = true;
                IsActive = true;
            }
        }

        private async Task<bool> TryLoadFromArchive(string backupZipFile)
        {
            using var archive = new ZipArchive(File.OpenRead(backupZipFile), ZipArchiveMode.Read);
            var wrpEntry = archive.GetEntry(parent.FileName);
            if (wrpEntry == null)
            {
                return false;
            }
            using (var worldStream = wrpEntry.Open())
            {
                parent.World = StreamHelper.Read<AnyWrp>(worldStream).GetEditableWrp();
            }

            var configEntry = archive.GetEntry("config.cpp");
            if (configEntry != null)
            {
                using var reader = new StreamReader(configEntry.Open());
                parent.ConfigFile = ConfigFileData.ReadFromContent(Path.GetFileNameWithoutExtension(parent.FileName), reader.ReadToEnd());
            }
            else
            {
                parent.ConfigFile = null;
            }

            var depEntry = archive.GetEntry("grma3-dependencies.json");
            if (depEntry != null)
            {
                var stream = depEntry.Open();
                parent.Dependencies = await JsonSerializer.DeserializeAsync<List<ModDependencyDefinition>>(stream) ?? new List<ModDependencyDefinition>();
            }

            return true;
        }
    }
}