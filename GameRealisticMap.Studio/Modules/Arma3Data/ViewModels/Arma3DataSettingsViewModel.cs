using System.ComponentModel.Composition;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using Gemini.Modules.Settings;

namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ISettingsEditor))]
    internal class Arma3DataSettingsViewModel : PropertyChangedBase, ISettingsEditor
    {
        private readonly IArma3DataModule _arma3;

        [ImportingConstructor]
        public Arma3DataSettingsViewModel(IArma3DataModule arma3)
        {
            _arma3 = arma3;
            UsePboProject = _arma3.UsePboProject;
        }

        public string SettingsPageName => GameRealisticMap.Studio.Labels.GeneralSettings;

        public string SettingsPagePath => "Arma 3";

        public string ProjectDriveBasePath => _arma3.ProjectDrive.MountPath;

        public string Arma3Path => Arma3ToolsHelper.GetArma3Path();

        public string Arma3ToolsPath => Arma3ToolsHelper.GetArma3ToolsPath();

        public string Arma3WorkshopPath => Arma3ToolsHelper.GetArma3WorkshopPath();

        public bool UsePboProject { get; set; }
        public bool UseBuiltinTool { get { return !UsePboProject; } set { UsePboProject = !value; } }

        public void ApplyChanges()
        {
            _arma3.UsePboProject = UsePboProject;
        }
    }
}
