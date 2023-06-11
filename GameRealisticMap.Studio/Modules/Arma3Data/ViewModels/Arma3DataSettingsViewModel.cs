using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Documents;
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
        }

        public string SettingsPageName => "Game data";

        public string SettingsPagePath => "Arma 3";

        public string ProjectDriveBasePath => _arma3.ProjectDrive.MountPath;

        public void ApplyChanges()
        {
        }
    }
}
