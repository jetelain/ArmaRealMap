using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using Gemini.Modules.Settings;

namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ISettingsEditor))]
    internal class Arma3ModsSettingsViewModel : PropertyChangedBase, ISettingsEditor
    {
        private readonly IArma3DataModule _arma3;
        private readonly IArma3ModsService _modsService;
        private List<ModSetting>? mods;

        [ImportingConstructor]
        public Arma3ModsSettingsViewModel(IArma3DataModule arma3, IArma3ModsService modsService)
        {
            _arma3 = arma3;
            _modsService = modsService;
        }

        public string SettingsPageName => "Mods";

        public string SettingsPagePath => "Arma 3";

        public List<ModSetting> Mods
        {
            get
            {
                if (mods == null)
                {
                    mods = GetMods();
                }
                return mods;
            }
        }

        private List<ModSetting> GetMods()
        {
            var used = _arma3.ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);

            return _modsService.GetModsList()
                .Select(d => new ModSetting(used.Contains(d.Path), d.Name, d.Path))
                .ToList();
        }

        public void ApplyChanges()
        {
            if ( mods != null)
            {
                _ = _arma3.ChangeActiveMods(mods.Where(m => m.IsActive).Select(m => m.Path));
            }
        }
    }
}
