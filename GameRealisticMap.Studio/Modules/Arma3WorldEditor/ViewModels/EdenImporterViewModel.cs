using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GameRealisticMap.Arma3.Edit;
using Gemini.Framework;
using NLog;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class EdenImporterViewModel : WindowBase, IProgress<double>
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("EdenImporter");

        private readonly Arma3WorldEditorViewModel parent;
        private bool _isWorking;
        private double _workingPercent;

        public EdenImporterViewModel(Arma3WorldEditorViewModel parent)
        {
            this.parent = parent;
        }

        public string ClipboardError { get; set; } = string.Empty;

        public string ClipboardWarning { get; set; } = string.Empty;

        public bool IsClipboardNotValid => !IsWorking && !string.IsNullOrEmpty(ClipboardError);

        public bool IsClipboardWarning => !IsWorking && string.IsNullOrEmpty(ClipboardError) && !string.IsNullOrEmpty(ClipboardWarning);

        public bool IsClipboardValid => !IsWorking && string.IsNullOrEmpty(ClipboardError) && string.IsNullOrEmpty(ClipboardWarning);

        public WrpEditBatch? Batch { get; set; }

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

        public Task ClipboardRefresh()
        {
            ClipboardError = Labels.CompositionClipboardInvalid;
            ClipboardWarning = string.Empty;

            var value = Clipboard.GetText();
            if (!string.IsNullOrEmpty(value))
            {
                IsWorking = true;
                WorkingPercent = 0.0;
                _ = Task.Run(() => DoDetect(value));
            }

            NotifyOfPropertyChange(nameof(IsClipboardValid));
            NotifyOfPropertyChange(nameof(IsClipboardWarning));
            NotifyOfPropertyChange(nameof(IsClipboardNotValid));
            NotifyOfPropertyChange(nameof(ClipboardError));
            NotifyOfPropertyChange(nameof(ClipboardWarning));

            return Task.CompletedTask;
        }

        private void DoDetect(string value)
        {
            using var task = new BasicProgressSystem(this, logger);
            try
            {
                var parser = new WrpEditBatchParser(task, parent.GameFileSystem);
                Batch = parser.ParseFromText(value);

                if (!string.IsNullOrEmpty(Batch.WorldName))
                {
                    ClipboardError = string.Empty;
                    var worldName = Path.GetFileNameWithoutExtension(parent.FilePath);
                    if (!string.Equals(Batch.WorldName, worldName, StringComparison.OrdinalIgnoreCase))
                    {
                        ClipboardWarning = string.Format("Exported data is for map '{0}' but current map is '{1}'", Batch.WorldName, worldName);
                    }
                    else if (Batch.Revision != (parent.ConfigFile?.Revision ?? 0))
                    {
                        ClipboardWarning = string.Format("Exported data is for revision '{0}' but current revision is '{1}'", Batch.Revision, parent.ConfigFile?.Revision);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                ClipboardError = e.Message;
            }

            IsWorking = false;
            NotifyOfPropertyChange(nameof(IsClipboardValid));
            NotifyOfPropertyChange(nameof(IsClipboardWarning));
            NotifyOfPropertyChange(nameof(IsClipboardNotValid));
            NotifyOfPropertyChange(nameof(ClipboardError));
            NotifyOfPropertyChange(nameof(ClipboardWarning));
        }

        public Task ClipboardImport()
        {
            if (Batch != null)
            {
                _ = Task.Run(() => parent.Apply(Batch));
            }
            return TryCloseAsync(true);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await ClipboardRefresh();
        }

        public Task Cancel() => TryCloseAsync(false);

        public void Report(double value)
        {
            WorkingPercent = value;
        }
    }
}