using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    internal abstract class BrowserViewModelBase : Document
    {
        protected readonly IArma3DataModule _arma3DataModule;
        protected readonly IArma3ModsService _modsService;
        private bool _isImporting;

        public List<ModInfo> ActiveMods { get; set; } = new List<ModInfo>();

        protected BrowserViewModelBase(IArma3DataModule arma3DataModule, IArma3ModsService arma3ModsService)
        {
            _arma3DataModule = arma3DataModule;
            _modsService = arma3ModsService;
            _arma3DataModule.Reloaded += Arma3DataModuleWasReloaded;
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                _arma3DataModule.Reloaded -= Arma3DataModuleWasReloaded;
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        private void Arma3DataModuleWasReloaded(object? sender, EventArgs e)
        {
            _ = Task.Run(() => UpdateActiveMods());
        }

        protected void UpdateActiveMods()
        {
            var allMods = _modsService.GetModsList();
            var active = _arma3DataModule.ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);
            ActiveMods = allMods.Where(m => active.Contains(m.Path)).ToList();
            NotifyOfPropertyChange(nameof(ActiveMods));
        }

        public bool IsImporting 
        {
            get { return _isImporting; }
            set { if (value != _isImporting) { _isImporting = value; NotifyOfPropertyChange(); } }
        }

        protected string filterText = string.Empty;
        public string FilterText
        {
            get { return filterText; }
            set
            {
                if (filterText != value)
                {
                    filterText = value;
                    RefreshFilter();
                }
                NotifyOfPropertyChange();
            }
        }

        protected abstract void RefreshFilter();
    }
}
