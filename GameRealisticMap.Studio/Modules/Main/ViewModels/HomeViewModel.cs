using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetConfigEditor;
using GameRealisticMap.Studio.Modules.MapConfigEditor;
using Gemini.Framework;
using Gemini.Framework.Services;
using Microsoft.Win32;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    internal class HomeViewModel : Document
    {
        private bool isArma3ToolsInstalled;
        private bool isArma3ToolsAccepted;
        private readonly IShell _shell;

        public HomeViewModel(IShell shell)
        {
            _shell = shell;
            DisplayName = "Home";
            SetupArma3WorkDriveCommand = new RelayCommand(_ => Arma3ToolsHelper.EnsureProjectDrive(false));
        }

        public bool IsArma3Installed { get; private set; }
        public bool IsArma3NotInstalled => !IsArma3Installed;
        public bool IsArma3ToolsInstalled => isArma3ToolsInstalled && isArma3ToolsAccepted;
        public bool IsArma3ToolsNotAccepted => !isArma3ToolsAccepted;
        public bool IsArma3ToolsNotInstalled => !isArma3ToolsInstalled;
        public bool IsPboProjectInstalled { get; private set; }
        public bool IsPboProjectNotInstalled => !IsPboProjectInstalled;
        public bool IsProjectDriveCreated { get; private set; }
        public bool IsProjectDriveNotCreated => !IsProjectDriveCreated;

        public RelayCommand SetupArma3WorkDriveCommand { get; }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await RefreshArma3ToolChain();
            await base.OnActivateAsync(cancellationToken);
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
            // TODO
        }

        public Task RefreshArma3ToolChain()
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
            IsPboProjectInstalled = File.Exists(Arma3ToolsHelper.GetPboProjectPath());
            IsProjectDriveCreated = Directory.Exists(Arma3ToolsHelper.GetProjectDrivePath());

            if (IsProjectDriveCreated)
            {
                // it's location may have changed
                IoC.Get<IArma3DataModule>().Reload();
            }

            NotifyOfPropertyChange(nameof(IsArma3Installed));
            NotifyOfPropertyChange(nameof(IsArma3NotInstalled));
            NotifyOfPropertyChange(nameof(IsArma3ToolsNotAccepted));
            NotifyOfPropertyChange(nameof(IsArma3ToolsInstalled));
            NotifyOfPropertyChange(nameof(IsArma3ToolsNotInstalled));
            NotifyOfPropertyChange(nameof(IsPboProjectInstalled));
            NotifyOfPropertyChange(nameof(IsPboProjectNotInstalled));
            NotifyOfPropertyChange(nameof(IsProjectDriveCreated));
            NotifyOfPropertyChange(nameof(IsProjectDriveNotCreated));

            return Task.CompletedTask;
        }
    }
}
