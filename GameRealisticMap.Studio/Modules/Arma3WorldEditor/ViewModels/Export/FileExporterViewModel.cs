using System;
using System.IO;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal class FileExporterViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldEditor;
        private FileExportMode _fileExportMode;
        private string _singleFilePath;
        private string _filePerKindPath;

        public FileExporterViewModel(Arma3WorldEditorViewModel worldEditor)
        {
            this.worldEditor = worldEditor;
            var target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GameRealisticMap", "Arma3", "TerrainBuilder", Path.GetFileNameWithoutExtension(worldEditor.FileName), "Export");
            _singleFilePath = Path.Combine(target, $"Revision{worldEditor.ConfigFile?.Revision ?? 1}.txt");
            _filePerKindPath = Path.Combine(target, $"Revision{worldEditor.ConfigFile?.Revision ?? 1}");
        }
        public Task Cancel() => TryCloseAsync(false);

        public FileExportMode FileExportMode { get { return _fileExportMode; } set { if (value != _fileExportMode) { _fileExportMode = value; NotifyOfPropertyChange(); } } }
        public string SingleFilePath { get { return _singleFilePath; } set { if (value != _singleFilePath) { _singleFilePath = value; NotifyOfPropertyChange(); } } }
        public string FilePerKindPath { get { return _filePerKindPath; } set { if (value != _filePerKindPath) { _filePerKindPath = value; NotifyOfPropertyChange(); } } }

        public async Task Export()
        {
            if (ProgressToolHelper.Start(
                    (FileExportMode == FileExportMode.SingleFile)
                    ? new FileExportSingleTask(_singleFilePath, worldEditor.World, worldEditor.Library)
                    : new FileExportPerKindTask(_filePerKindPath, worldEditor.World, worldEditor.Library)))
            {
                await TryCloseAsync(false);
            }
        }
    }
}
