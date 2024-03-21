using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Caliburn.Micro;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Toolkit;
using NLog;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export(typeof(GdtBrowserViewModel))]
    internal class GdtBrowserViewModel : BrowserViewModelBase
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("GdtBrowser");

        private readonly IArma3Previews _arma3Previews;
        private readonly IGdtCatalogStorage _catalogService;
        private bool isSaving;

        [ImportingConstructor]
        public GdtBrowserViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IGdtCatalogStorage catalogService, IArma3ModsService arma3ModsService)
            : base(arma3DataModule, arma3ModsService)
        {
            _arma3Previews = arma3Previews;
            _catalogService = catalogService;
            DisplayName = "Ground Detail Texture Library";
        }

        public BindableCollection<GdtDetailViewModel>? AllItems { get; private set; }

        public ICollectionView? Items { get; private set; }

        public IArma3Previews Previews => _arma3Previews;

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            AllItems = new BindableCollection<GdtDetailViewModel>();
            Items = CollectionViewSource.GetDefaultView(AllItems);
            Items.SortDescriptions.Add(new SortDescription(nameof(GdtDetailViewModel.DisplayName), ListSortDirection.Ascending));
            Items.Filter = (item) => Filter((GdtDetailViewModel)item);

            NotifyOfPropertyChange(nameof(Items));

            _ = Task.Run(() => DoLoad());

            _catalogService.Updated += CatalogServiceUpdated;

            return Task.CompletedTask;
        }

        private void CatalogServiceUpdated(object? sender, List<GdtCatalogItem> catalogItems)
        {
            var items = AllItems;
            if (!isSaving && items != null)
            {
                foreach (var item in catalogItems)
                {
                    var existing = items.FirstOrDefault(i => string.Equals(i.ColorTexture, item.Material.ColorTexture, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.SyncCatalog(item);
                    }
                    else
                    {
                        items.Add(new GdtDetailViewModel(this, item));
                    }
                }
            }
        }

        private async Task DoLoad()
        {
            IsImporting = true;
            try
            {
                var items = await _catalogService.GetOrLoad();
                AllItems?.AddRange(items.Select(m => new GdtDetailViewModel(this, m)));
                UpdateActiveMods();
            }
            catch (Exception ex)
            {
            }

            IsImporting = false;
        }

        public Task ImportA3()
        {
            Task.Run(async () =>
            {
                IsImporting = true;
                await _catalogService.ImportVanilla();
                IsImporting = false;
            });
            return Task.CompletedTask;
        }

        public Task ImportARM()
        {
            DoImportMod("2982306133");
            return Task.CompletedTask;
        }

        private void DoImportMod(string steamId)
        {
            var installed = _modsService.GetMod(steamId);
            if (installed == null)
            {
                // Not installed
                ShellHelper.OpenUri("steam://url/CommunityFilePage/" + steamId);
                return;
            }
            if (!ActiveMods.Any(m => m.SteamId == steamId))
            {
                // Not active, activate him
                _arma3DataModule.ChangeActiveMods(_arma3DataModule.ActiveMods.Concat(new[] { installed.Path }));
            }
            Task.Run(async () =>
            {
                IsImporting = true;
                await _catalogService.ImportMod(installed);
                IsImporting = false;
            });
        }

        private async Task SaveChanges()
        {
            var items = AllItems;
            if (items != null)
            {
                isSaving = true;
                await _catalogService.SaveChanges(items.Select(i => i.ToDefinition()).ToList());
                isSaving = false;
            }
        }

        private IEnumerable<Color> GetUsedColors()
        {
            return AllItems?.Select(i => i.ColorId) ?? Enumerable.Empty<Color>();
        }

        private bool Filter(GdtDetailViewModel item)
        {
            if (!string.IsNullOrEmpty(filterText))
            {
                return item.DisplayName.Contains(filterText, StringComparison.OrdinalIgnoreCase) 
                    || item.ColorTexture.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        protected override void RefreshFilter()
        {
            Items?.Refresh();
        }
    }
}
