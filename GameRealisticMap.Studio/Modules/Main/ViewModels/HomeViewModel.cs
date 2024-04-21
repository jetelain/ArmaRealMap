using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;
using GameRealisticMap.Studio.Modules.AssetConfigEditor;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Modules.MapConfigEditor;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Win32;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    [Export]
    internal class HomeViewModel : Document
    {
        private bool isArma3ToolsInstalled;
        private bool isArma3ToolsAccepted;
        private readonly IShell _shell;
        private readonly IArma3RecentHistory _history;
        private readonly IRecentFilesService _recentFilesService;

        [ImportingConstructor]
        public HomeViewModel(IShell shell, IArma3RecentHistory history, IRecentFilesService recentFilesService)
        {
            _shell = shell;
            _history = history;
            _history.Changed += (_, _) => UpdateRecent();

            _recentFilesService = recentFilesService;
            _recentFilesService.Changed += (_, _) => UpdateRecent();

            DisplayName = Labels.Home;
            SetupArma3WorkDriveCommand = new RelayCommand(_ => Arma3ToolsHelper.EnsureProjectDrive(false));
        }

        public bool IsArma3Installed { get; private set; }
        public bool IsArma3NotInstalled => !IsArma3Installed;
        public bool IsArma3ToolsInstalled => isArma3ToolsInstalled && isArma3ToolsAccepted;
        public bool IsArma3ToolsNotAccepted => !isArma3ToolsAccepted;
        public bool IsArma3ToolsNotInstalled => !isArma3ToolsInstalled;
        public bool IsProjectDriveCreated { get; private set; }
        public bool IsProjectDriveNotCreated => !IsProjectDriveCreated;

        public RelayCommand SetupArma3WorkDriveCommand { get; }
        public List<WorldEntry> EditMaps { get; private set; } = new List<WorldEntry>();
        public List<RecentEntry> RecentFiles { get; private set; } = new List<RecentEntry>();

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            await RefreshArma3ToolChain();
            UpdateRecent();
        }

        private void UpdateRecent()
        {
            _ = Task.Run(async () =>
            {
                if (IsProjectDriveCreated)
                {
                    var a3maps = await _history.GetEntries();
                    if (a3maps.Count > 0)
                    {
                        var drive = IoC.Get<IArma3DataModule>().ProjectDrive;
                        EditMaps = a3maps.Select(r => new WorldEntry(r, drive)).Where(r => r.Exists).OrderByDescending(r => r.TimeStamp).Take(10).ToList();
                        NotifyOfPropertyChange(nameof(EditMaps));
                    }
                }
                var result = await _recentFilesService.GetEntries();
                if (result.Count > 0)
                {
                    RecentFiles = result.Select(r => new RecentEntry(r)).Where(r => r.Exists && !EditMaps.Any(m => r.IsSame(m.FilePath))).OrderByDescending(r => r.IsPinned).ThenByDescending(r => r.TimeStamp).Take(10).ToList();
                    NotifyOfPropertyChange(nameof(RecentFiles));
                }
            });
        }

        public async Task NewArma3MapConfig()
        {
            await Create(IoC.Get<MapConfigEditorProvider>());
        }

        private async Task Create(IEditorProvider editorProvider)
        {
            var doc = editorProvider.Create();
            await editorProvider.New(doc, "Untitled" + editorProvider.FileTypes.First().FileExtension);
            await _shell.OpenDocumentAsync(doc);
        }

        public async Task NewArma3AssetConfig()
        {
            await Create(IoC.Get<AssetConfigEditorProvider>());
        }

        public async Task BrowseArma3Assets()
        {
            await _shell.OpenDocumentAsync(IoC.Get<AssetBrowserViewModel>());
        }

        public async Task BrowseArma3Gdt()
        {
            await _shell.OpenDocumentAsync(IoC.Get<GdtBrowserViewModel>());
        }

        public async Task RefreshArma3ToolChain()
        {
            IsArma3Installed = !string.IsNullOrEmpty(Arma3ToolsHelper.GetArma3Path());
            isArma3ToolsInstalled = !string.IsNullOrEmpty(Arma3ToolsHelper.GetArma3ToolsPath());
            if (isArma3ToolsInstalled)
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Bohemia Interactive\arma 3 tools"))
                {
                    isArma3ToolsAccepted = key != null && !string.IsNullOrEmpty(key.GetValue("license") as string);
                }
            }
            else
            {
                isArma3ToolsAccepted = false;
            }
            
            IsProjectDriveCreated = Directory.Exists(Arma3ToolsHelper.GetProjectDrivePath());

            NotifyOfPropertyChange(nameof(IsArma3Installed));
            NotifyOfPropertyChange(nameof(IsArma3NotInstalled));
            NotifyOfPropertyChange(nameof(IsArma3ToolsNotAccepted));
            NotifyOfPropertyChange(nameof(IsArma3ToolsInstalled));
            NotifyOfPropertyChange(nameof(IsArma3ToolsNotInstalled));
            NotifyOfPropertyChange(nameof(IsProjectDriveCreated));
            NotifyOfPropertyChange(nameof(IsProjectDriveNotCreated));

            if (IsProjectDriveCreated)
            {
                // it's location may have changed
                await IoC.Get<IArma3DataModule>().Reload();
            }
        }
    }
}
