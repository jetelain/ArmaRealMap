using System.ComponentModel.Composition;
using Caliburn.Micro;
using Gemini.Modules.Settings;

namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    [Export(typeof(ISettingsEditor))]
    internal class Arma3DataSettingsViewModel : PropertyChangedBase, ISettingsEditor
    {
        private readonly Arma3DataModule _arma3;

        [ImportingConstructor]
        public Arma3DataSettingsViewModel(Arma3DataModule arma3)
        {
            _arma3 = arma3;
        }

        public string SettingsPageName => "Arma 3";

        public string SettingsPagePath => "Game data";

        public void ApplyChanges()
        {
        }
    }
}
