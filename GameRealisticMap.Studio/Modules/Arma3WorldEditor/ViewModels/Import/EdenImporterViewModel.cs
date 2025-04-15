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
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Toolkit;
using NLog;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Import
{
    internal class EdenImporterViewModel : ModalProgressBase
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("EdenImporter");

        private readonly Arma3WorldEditorViewModel parent;
        private SlopeLandContactBehavior _slopeLandContactBehavior = SlopeLandContactBehavior.TryToCompensate;

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

        public bool LaunchDependenciesOnly { get; set; } = true;

        public bool IsReadyToImport => Batch != null && Batch.IsComplete && string.IsNullOrEmpty(ClipboardError);

        public SlopeLandContactBehavior SlopeLandContactBehavior
        {
            get { return _slopeLandContactBehavior; }
            set
            {
                if (value != _slopeLandContactBehavior)
                {
                    _slopeLandContactBehavior = value;
                    NotifyOfPropertyChange();

                    ClipboardRefresh();
                }
            }
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
                var parser = new WrpEditBatchParser(task, parent.Library);

                var batch = parser.ParseFromText(value, _slopeLandContactBehavior);
                if (!string.IsNullOrEmpty(batch.WorldName))
                {
                    ClipboardError = string.Empty;
                    var worldName = Path.GetFileNameWithoutExtension(parent.FilePath);
                    if (!string.Equals(batch.WorldName, worldName, StringComparison.OrdinalIgnoreCase))
                    {
                        ClipboardWarning = string.Format(Labels.ExportedDataMapMismatch, batch.WorldName, worldName);
                    }
                    else if (batch.Revision != (parent.ConfigFile?.Revision ?? 0))
                    {
                        ClipboardWarning = string.Format(Labels.ExportedDataRevisionMismatch, batch.Revision, parent.ConfigFile?.Revision);
                    }

                    if (batch.IsComplete)
                    {
                        Batch = batch;
                    }
                    else
                    {
                        if (Batch == null || Batch.IsComplete)
                        {
                            Batch = batch;
                        }
                        else
                        {
                            var intersect = Batch.PartIndexes.Intersect(batch.PartIndexes).ToList();
                            if (intersect.Any())
                            {
                                ClipboardError = string.Format(Labels.PartHasBeenAlreadyImported, intersect.First());
                            }
                            else
                            {
                                Batch.PartIndexes.AddRange(batch.PartIndexes);
                                Batch.Add.AddRange(batch.Add);
                                Batch.Remove.AddRange(batch.Remove);
                                Batch.Elevation.AddRange(batch.Elevation);
                            }
                        }
                        if (!Batch.IsComplete)
                        {
                            ClipboardWarning = GetNextPartMessage(Batch);
                        }
                    }

                    if (Batch != null)
                    {
                        ClipboardMessage = string.Format(Labels.ExportedDataStats,
                            Batch.Add.Count,
                            Batch.Remove.Count,
                            Batch.Elevation.Count);
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
            NotifyOfPropertyChange(nameof(IsReadyToImport));
            NotifyOfPropertyChange(nameof(ClipboardError));
            NotifyOfPropertyChange(nameof(ClipboardWarning));
            NotifyOfPropertyChange(nameof(ClipboardMessage));
        }

        private static string GetNextPartMessage(WrpEditBatch batch)
        {
            var missing = Enumerable.Range(0, batch.PartCount ?? 1).Except(batch.PartIndexes).ToList();
            if (missing.Count == 0)
            {
                return string.Empty;
            }
            return string.Format(Labels.ImportNextPartPrompt, missing.Min() + 1);
        }

        public Task ClipboardImport()
        {
            if (Batch != null)
            {
                parent.Apply(Batch);
            }
            return TryCloseAsync(true);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await ClipboardRefresh();
        }

        public Task LaunchArma3()
        {
            var installed = IoC.Get<IArma3ModsService>().GetModsList();
            if (!IsInstalled(installed, "3016661145"))
            {
                ShellHelper.OpenUri("steam://url/CommunityFilePage/3016661145");
                return Task.CompletedTask;
            }

            List< ModDependencyDefinition> dependencies;

            if (LaunchDependenciesOnly)
            {
                dependencies = parent.Dependencies.Items.ToList();
            }
            else
            {
                var used = IoC.Get<IArma3DataModule>().ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);

                dependencies = installed.Where(i => !string.IsNullOrEmpty(i.SteamId) && used.Contains(i.Path))
                    .Select(i => new ModDependencyDefinition(i.SteamId!, i.IsCdlc ? Path.GetFileName(i.Path) : null))
                    .ToList();
            }

            dependencies.Add(new ModDependencyDefinition("450814997")); // CBA3 (required by Export to GameRealisticMap)
            dependencies.Add(new ModDependencyDefinition("3016661145")); // Export to GameRealisticMap
            AddIfInstalled(installed, dependencies, "882231372"); // Eden Extended Objects
            AddIfInstalled(installed, dependencies, "1923321700"); // O&T Expansion Eden
            AddIfInstalled(installed, dependencies, "2822758266"); // Deformer
            AddIfInstalled(installed, dependencies, "623475643"); // 3den Enhanced

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