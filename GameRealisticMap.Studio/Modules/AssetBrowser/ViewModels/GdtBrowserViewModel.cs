using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine.Materials;
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
        private readonly Task<BindableCollection<GdtDetailViewModel>> awaitableItems;
        private bool isSaving;

        [ImportingConstructor]
        public GdtBrowserViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IGdtCatalogStorage catalogService, IArma3ModsService arma3ModsService)
            : base(arma3DataModule, arma3ModsService)
        {
            AllItems = new BindableCollection<GdtDetailViewModel>();

            _arma3Previews = arma3Previews;
            _catalogService = catalogService;
            _catalogService.Updated += CatalogServiceUpdated;
            DisplayName = "Ground Detail Texture Library";
            awaitableItems = Task.Run(DoLoad);
        }

        public BindableCollection<GdtDetailViewModel> AllItems { get; private set; }

        public ICollectionView? Items { get; private set; }

        public IArma3Previews Previews => _arma3Previews;

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            Items = CollectionViewSource.GetDefaultView(AllItems);
            Items.SortDescriptions.Add(new SortDescription(nameof(GdtDetailViewModel.DisplayName), ListSortDirection.Ascending));
            Items.Filter = (item) => Filter((GdtDetailViewModel)item);

            return base.OnActivateAsync(cancellationToken);
        }

        private void CatalogServiceUpdated(object? sender, List<GdtCatalogItem> catalogItems)
        {
            if (isSaving)
            {
                return;
            }

            var items = AllItems;
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

        private async Task<BindableCollection<GdtDetailViewModel>> DoLoad()
        {
            IsImporting = true;
            try
            {
                var items = await _catalogService.GetOrLoad();
                AllItems.AddRange(items.Select(m => new GdtDetailViewModel(this, m)));
                UpdateActiveMods();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            IsImporting = false;

            return AllItems;
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
            isSaving = true;
            await _catalogService.SaveChanges(AllItems.Select(i => i.ToDefinition()).ToList());
            isSaving = false;
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

        internal async Task<GdtDetailViewModel?> ImportExternal(TerrainMaterial terrainMaterial, SurfaceConfig? surfaceConfig)
        {
            if (surfaceConfig == null)
            {
                return null;
            }
            var vm = new GdtDetailViewModel(this, new GdtCatalogItem(terrainMaterial, surfaceConfig));
            (await awaitableItems).Add(vm);
            return vm;
        }

        internal async Task<GdtDetailViewModel?> Resolve(TerrainMaterial mat, SurfaceConfig? surf)
        {
            var items = await awaitableItems;
            var libraryItem = items.FirstOrDefault(i => string.Equals(i.ColorTexture, mat.ColorTexture, StringComparison.OrdinalIgnoreCase));
            if (libraryItem == null)
            {
                libraryItem = await ImportExternal(mat, surf);
            }
            return libraryItem;
        }
    }
}
