using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit;
using GameRealisticMap.Arma3.GameLauncher;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Toolkit;
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
        public string ClipboardMessage { get; set; } = string.Empty;

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
            ClipboardMessage = string.Empty;

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
            NotifyOfPropertyChange(nameof(ClipboardMessage));

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
                    ClipboardMessage = string.Format("{0} objects to add, {1} objects to remove, {2} elevation grid changes", 
                        Batch.Add.Count,
                        Batch.Remove.Count,
                        Batch.Elevation.Count);
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
            NotifyOfPropertyChange(nameof(ClipboardMessage));
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

        public Task LaunchArma3()
        {
            var installed = IoC.Get<IArma3ModsService>().GetModsList();
            if (!IsInstalled(installed, "3016661145"))
            {
                ShellHelper.OpenUri("steam://url/CommunityFilePage/3016661145");
                return Task.CompletedTask;
            }
            var dependencies = parent.Dependencies.ToList();
            dependencies.Add(new ModDependencyDefinition("450814997")); // CBA3 (required by Export to GameRealisticMap)
            dependencies.Add(new ModDependencyDefinition("3016661145")); // Export to GameRealisticMap
            AddIfInstalled(installed, dependencies, "882231372"); // Eden Extended Objects
            AddIfInstalled(installed, dependencies, "1923321700"); // O&T Expansion Eden
            AddIfInstalled(installed, dependencies, "2822758266"); // Deformer
            // AddIfInstalled(installed, dependencies, "1736812763"); // Arma 3 Non Editor Objects ???

            Arma3Helper.Launch(dependencies, parent.TargetModDirectory, Path.GetFileNameWithoutExtension(parent.FileName));
            return Task.CompletedTask;
        }

        private static void AddIfInstalled(List<ModInfo> installed, List<ModDependencyDefinition> dependencies, string steamId)
        {
            if (IsInstalled(installed, steamId))
            {
                dependencies.Add(new ModDependencyDefinition(steamId));
            }
        }

        private static bool IsInstalled(List<ModInfo> installed, string steamId)
        {
            return installed.Any(m => m.SteamId == steamId);
        }
    }
}