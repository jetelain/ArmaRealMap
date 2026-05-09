using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using Gemini.Modules.Settings;

namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ISettingsEditorAsync))]
    internal class Arma3DataSettingsViewModel : PropertyChangedBase, ISettingsEditorAsync
    {
        private readonly IArma3DataModule _arma3;

        [ImportingConstructor]
        public Arma3DataSettingsViewModel(IArma3DataModule arma3)
        {
            _arma3 = arma3;
            UsePboProject = _arma3.UsePboProject;
            ProjectDriveBasePath = _arma3.ProjectDriveBasePath;
        }

        public string SettingsPageName => GameRealisticMap.Studio.Labels.GeneralSettings;

        public string SettingsPagePath => "Arma 3";

        public string ProjectDriveBasePath { get; set; }

        public string ProjectDriveMountPath => _arma3.ProjectDrive.MountPath;

        public string Arma3Path => Arma3ToolsHelper.GetArma3Path();

        public string Arma3ToolsPath => Arma3ToolsHelper.GetArma3ToolsPath();

        public string Arma3WorkshopPath => Arma3ToolsHelper.GetArma3WorkshopPath();

        public bool UsePboProject { get; set; }

        public bool UseBuiltinTool { get { return !UsePboProject; } set { UsePboProject = !value; } }

        public bool IsPboProjectInstalled => File.Exists(Arma3ToolsHelper.GetPboProjectPath());

        public async Task ApplyChangesAsync()
        {
            _arma3.UsePboProject = UsePboProject;
            await _arma3.SetProjectDriveBasePath(ProjectDriveBasePath);
            NotifyOfPropertyChange(nameof(ProjectDriveMountPath));
        }
    }
}
