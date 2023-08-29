using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class FileImporterViewModel : WindowBase, IProgress<double>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("FileImporter");

        private readonly Arma3WorldEditorViewModel parent;
        private readonly List<TerrainBuilderObject> list = new List<TerrainBuilderObject>();
        private readonly string fileName;
        private bool _isWorking;
        private double _workingPercent;
        private bool isAbsolute;

        public FileImporterViewModel(Arma3WorldEditorViewModel parent, string fileName)
        {
            this.parent = parent;
            this.fileName = fileName;
        }

        public Task Cancel() => TryCloseAsync(false);

        public bool IsWorking
        {
            get { return _isWorking; }
            set { _isWorking = value; NotifyOfPropertyChange(); }
        }

        public double WorkingPercent
        {
            get { return _workingPercent; }
            set { _workingPercent = value; NotifyOfPropertyChange(); }
        }

        public string Error { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsNotValid => !IsWorking && !string.IsNullOrEmpty(Error);

        public bool IsValid => !IsWorking && string.IsNullOrEmpty(Error);

        public string FileName => fileName;

        public bool IsAbsolute
        {
            get { return isAbsolute; }
            set { if (isAbsolute != value) { isAbsolute = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(IsRelative)); } }
        }

        public bool IsRelative { get { return !IsAbsolute; } set { IsAbsolute = !value; } }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            IsWorking = true;
            NotifyOfPropertyChange(nameof(IsNotValid));
            NotifyOfPropertyChange(nameof(IsValid));

            _ = Task.Run(() => DoReadFile());

            return Task.CompletedTask;
        }

        private async Task DoReadFile()
        {

            try
            {
                var library = new ImportLibrary(await IoC.Get<IAssetsCatalogService>().Load(), parent.Library);

                list.Clear();
                using (var task = new BasicProgressSystem(this, logger))
                {
                    var lines = await File.ReadAllLinesAsync(fileName);
                    foreach (var line in lines.ProgressStep(task, "Lines"))
                    {
                        var csvLine = line.Trim().Split(';');
                        if (csvLine.Length > 7)
                        {
                            list.Add(new TerrainBuilderObject(ElevationMode.Absolute, csvLine, library));
                        }
                    }
                }
                if (list.Count == 0)
                {
                    Error = GameRealisticMap.Studio.Labels.NoValidObjectDefinitionFoundInFile;
                }
                else
                {
                    Message = string.Format(GameRealisticMap.Studio.Labels.NumObjectsToImport, list.Count);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Error = ex.Message;
            }

            IsWorking = false;

            NotifyOfPropertyChange(nameof(Error));
            NotifyOfPropertyChange(nameof(IsNotValid));
            NotifyOfPropertyChange(nameof(IsValid));
            NotifyOfPropertyChange(nameof(Message));

            if (list.All(o => o.Elevation == 0))
            {
                IsRelative = true;
            }
            else
            {
                IsAbsolute = true;
            }
        }

        public void Report(double value)
        {
            WorkingPercent = value;
        }

        public Task Import()
        {
            if (IsRelative)
            {
                parent.Apply(list.Select(i => new TerrainBuilderObject(i.Model, i.Point, i.Elevation, ElevationMode.Relative, i.Yaw, i.Pitch, i.Roll, i.Scale)).ToList());
            }
            else
            {
                parent.Apply(list);
            }
            return TryCloseAsync(true);
        }
    }
}