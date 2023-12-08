using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal class FileImporterViewModel : ModalProgressBase
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetLogger("FileImporter");

        private readonly Arma3WorldEditorViewModel parent;
        private readonly List<TerrainBuilderObject> list = new List<TerrainBuilderObject>();
        private readonly string fileName;
        private bool isAbsolute;
        private List<AmbiguousItem> ambigousModels = new List<AmbiguousItem>();

        public FileImporterViewModel(Arma3WorldEditorViewModel parent, string fileName)
        {
            this.parent = parent;
            this.fileName = fileName;
        }

        public string Message { get; set; } = string.Empty;

        public string FileName => fileName;

        public bool IsAbsolute
        {
            get { return isAbsolute; }
            set { if (isAbsolute != value) { isAbsolute = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(IsRelative)); } }
        }

        public bool IsRelative { get { return !IsAbsolute; } set { IsAbsolute = !value; } }

        public bool HasAmbigousModels => AmbigousModels.Count > 0;

        public List<AmbiguousItem> AmbigousModels
        {
            get { return ambigousModels; }
            set
            {
                ambigousModels = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(HasAmbigousModels));
            }
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            IsWorking = true;
            AmbigousModels = new List<AmbiguousItem>();

            _ = Task.Run(() => DoReadFile());

            return Task.CompletedTask;
        }

        private async Task DoReadFile()
        {

            try
            {
                var library = new ImportLibrary(await IoC.Get<IAssetsCatalogService>().GetOrLoad(), parent.Library);

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
                    Error = Labels.NoValidObjectDefinitionFoundInFile;
                }
                else
                {
                    Message = string.Format(Labels.NumObjectsToImport, list.Count);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                Error = ex.Message;
                if (ex is AmbiguousModelName amn)
                {
                    await ShowAmbiguousItems(amn);
                }
            }

            IsWorking = false;

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

        private async Task ShowAmbiguousItems(AmbiguousModelName amn)
        {
            var preview = IoC.Get<IArma3Previews>();
            var items = new List<AmbiguousItem>();
            foreach (var path in amn.Candidates)
            {
                items.Add(new AmbiguousItem(this, amn.Name, path, await preview.GetPreview(path)));
            }
            AmbigousModels = items;
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

        internal Task Resolve(string name, string path)
        {
            parent.Library.TryRegister(name, path);

            return OnActivateAsync(CancellationToken.None);
        }
    }
}