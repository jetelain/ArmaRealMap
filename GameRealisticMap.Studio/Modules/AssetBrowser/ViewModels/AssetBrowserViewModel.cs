using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export(typeof(AssetBrowserViewModel))]
    internal class AssetBrowserViewModel : Document
    {
        private readonly IArma3DataModule _arma3DataModule;
        private readonly IArma3Previews _arma3Previews;
        private readonly IAssetsCatalogService _catalogService;

        public BindableCollection<AssetViewModel>? AllAssets { get; set; }
        public ICollectionView? Assets { get; set; }

        public string AssetsCatalogPath { get; set; } = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "assets.json");

        [ImportingConstructor]
        public AssetBrowserViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IAssetsCatalogService catalogService)
        {
            _arma3DataModule = arma3DataModule;
            _arma3Previews = arma3Previews;
            _catalogService = catalogService;
            DisplayName = "Asset Browser";
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var items = await _catalogService.LoadFrom(AssetsCatalogPath);

            var known = new HashSet<string>(items.Select(i => i.Path), StringComparer.OrdinalIgnoreCase);
            var missing = await _catalogService.ImportItems(_arma3DataModule.Library.Models.Where(m => !known.Contains(m.Path)).Select(m => m.Path));
            items.AddRange(missing);
            if (missing.Count > 0)
            {
                await _catalogService.SaveTo(AssetsCatalogPath, items);
            }

            AllAssets = new BindableCollection<AssetViewModel>(items.Select(m => new AssetViewModel(m, _arma3Previews.GetPreviewFast(m.Path))));
            Assets = CollectionViewSource.GetDefaultView(AllAssets);
            Assets.SortDescriptions.Add(new SortDescription(nameof(AssetViewModel.Name), ListSortDirection.Ascending));
            Assets.Filter = (item) => Filter((AssetViewModel)item);

            NotifyOfPropertyChange(nameof(Assets));

            var assets = AllAssets.Where(a => !a.Preview.IsFile).ToList();
            _ = Task.Run(() => LoadPreviews(assets));
        }

        private async Task LoadPreviews(List<AssetViewModel> assets)
        {
            foreach(var asset in assets)
            {
                asset.Preview = await _arma3Previews.GetPreview(asset.Path);
            }
        }

        private bool Filter(AssetViewModel item)
        {
            if( !string.IsNullOrEmpty(filterText))
            {
                return item.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) || item.Path.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        private string filterText = string.Empty;
        public string FilterText
        {
            get { return filterText; }
            set
            {
                if (filterText != value)
                {
                    filterText = value;
                    Assets?.Refresh();
                }
                NotifyOfPropertyChange();
            }
        }

    }
}
