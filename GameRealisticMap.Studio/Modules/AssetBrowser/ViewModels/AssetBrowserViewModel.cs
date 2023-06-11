using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Data;
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
        private readonly IArma3ModsService _modsService;

        public BindableCollection<AssetViewModel>? AllAssets { get; set; }
        public ICollectionView? Assets { get; set; }

        public BindableCollection<ModOption> Mods { get; } = new BindableCollection<ModOption>();

        public List<CategoryOption> Categories { get; } = new List<CategoryOption>();
        public List<CategoryOption> SetCategories { get; } = new List<CategoryOption>();

        public List<ModInfo> ActiveMods { get; set; } = new List<ModInfo>();

        public List<ModInfo> UsualMods { get; set; } = new List<ModInfo>() {
            new ModInfo("CUP", string.Empty, "583496184"),
            new ModInfo("JBAD", string.Empty, "520618345"),
            new ModInfo("Em Buildings", string.Empty, "671539540"),
            new ModInfo("Mount Buildings", string.Empty, "2782382831"),
            new ModInfo("RHS TERRACORE", string.Empty, "2288691268"),
            new ModInfo("ARM", string.Empty, "2982306133")
        };

        public string AssetsCatalogPath { get; set; } = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "Arma3",
            "assets.json");

        [ImportingConstructor]
        public AssetBrowserViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IAssetsCatalogService catalogService, IArma3ModsService arma3ModsService)
        {
            _arma3DataModule = arma3DataModule;
            _arma3Previews = arma3Previews;
            _catalogService = catalogService;
            _modsService = arma3ModsService;

            DisplayName = "Asset Browser - Arma 3";
            Mods.Add(new ModOption("All", ""));
            SetCategories.AddRange(Enum.GetValues<AssetCatalogCategory>().Select(c => new CategoryOption(c.ToString(), c)).OrderBy(c => c.Name));
            Categories.Add(new CategoryOption("All", null));
            Categories.AddRange(SetCategories);

            ImportA3 = new RelayCommand(DoImportA3);
            ImportUsualMod = new RelayCommand(DoImportUsualMod);
            ImportActiveMod = new RelayCommand(DoImportActiveMod);

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
            _ = Task.Run(() => LoadMods());
        }

        private void DoImportUsualMod(object obj)
        {
            var mod = (ModInfo)obj;
            if (!ActiveMods.Any(m => m.SteamId == mod.SteamId))
            { 
                var installed = _modsService.GetMod(mod.SteamId!);
                if ( installed == null )
                {
                    // Not installed
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = "steam://url/CommunityFilePage/" + mod.SteamId });
                    return;
                }
                // Not active, activate him
                _arma3DataModule.ChangeActiveMods(_arma3DataModule.ActiveMods.Concat(new[] { installed.Path }));
            }
            Task.Run(() => DoImportBuiltin(mod.SteamId!, mod.Name));
        }

        private void DoImportActiveMod(object obj)
        {
            Task.Run(() => DoImportActiveMod((ModInfo)obj));
        }

        private void DoImportA3(object obj)
        {
            Task.Run(() => DoImportBuiltin((string)obj, string.Empty));
        }

        private async Task DoImportActiveMod(ModInfo mod)
        {
            var usual = UsualMods.FirstOrDefault(u => u.SteamId == mod.SteamId);
            if (usual != null && usual.SteamId != null)
            {
                await DoImportBuiltin(usual.SteamId, usual.Name).ConfigureAwait(false);
                return;
            }
            var pbo = new PboFileSystem(Enumerable.Empty<string>(), new[] { mod.Path });
            var models = pbo.FindAll($"*.p3d").ToList();
            var screenShots = pbo.FindAll($"land_*.jpg").ToList();
            if (screenShots.Count > 0)
            {
                var screenShotsModels = screenShots.Select(o => "\\" + System.IO.Path.GetFileNameWithoutExtension(o).Substring(5) + ".p3d").ToList();
                var modelsWithScreenshot = models.Where(m => screenShotsModels.Any(s => m.EndsWith(s, StringComparison.OrdinalIgnoreCase))).ToList();
                await DoImport(modelsWithScreenshot, mod.Name).ConfigureAwait(false);
            }
            else
            {
                await DoImport(models, mod.Name).ConfigureAwait(false);
            }
        }

        private async Task DoImportBuiltin(string obj, string modId)
        {
            await DoImport(BuiltinObjectsList.Read(obj), modId).ConfigureAwait(false);
        }

        private async Task DoImport(List<string> paths, string modId)
        {
            IsImporting = true; 
            NotifyOfPropertyChange(nameof(IsImporting));
            var target = AllAssets!;
            var existing = target.Select(a => a.Path)?.ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();
            var needImport = paths.Where(p => !existing.Contains(p)).ToList();
            var imported = await _catalogService.ImportItems(needImport, modId).ConfigureAwait(false);
            var models = imported.Select(m => new AssetViewModel(m, _arma3Previews.GetPreviewFast(m.Path))).ToList();
            target.AddRange(models);
            UpdateModsList(models);
            await _catalogService.SaveTo(AssetsCatalogPath, target.Select(i => i.Item).ToList());
            IsImporting = false;
            NotifyOfPropertyChange(nameof(IsImporting));
            await LoadPreviews(models.Where(a => !a.Preview.IsFile).ToList());
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

            UpdateModsList(AllAssets);

            var assets = AllAssets.Where(a => !a.Preview.IsFile).ToList();
            _ = Task.Run(() => LoadPreviews(assets));
            _ = Task.Run(() => LoadMods());

            if (items.Count == 0)
            {
                _ = Task.Run(async () => { await DoImportBuiltin("Base", string.Empty); await DoImportBuiltin("Livonia", string.Empty); await DoImportBuiltin("Tanoa", string.Empty); });
            }

        }

        private void UpdateModsList(IEnumerable<AssetViewModel> assets)
        {
            var existingMods = Mods.Select(m => m.ModId).ToHashSet();

            Mods.AddRange(assets
                .Select(a => a.ModId)
                .Distinct()
                .Where(m => !existingMods.Contains(m))
                .OrderBy(m => m)
                .Select(m => new ModOption(m, m)));
        }

        private async Task LoadPreviews(List<AssetViewModel> assets)
        {
            foreach(var asset in assets)
            {
                asset.Preview = await _arma3Previews.GetPreview(asset.Path);
            }
        }
        private void LoadMods()
        {
            var allMods = _modsService.GetModsList();
            var active = _arma3DataModule.ActiveMods.ToHashSet(StringComparer.OrdinalIgnoreCase);
            ActiveMods = allMods.Where(m => active.Contains(m.Path)).ToList();
            NotifyOfPropertyChange(nameof(ActiveMods));
        }

        private bool Filter(AssetViewModel item)
        {
            if (filterCategory != null && item.Category != filterCategory.Value)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(filterModId) && item.ModId != filterModId)
            {
                return false;
            }

            if ( !string.IsNullOrEmpty(filterText))
            {
                return item.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase) || item.Path.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            }
            return true;
        }

        internal async Task ChangeAssetsCategoryAsync(IEnumerable<AssetViewModel> enumerable, AssetCatalogCategory? category)
        {
            foreach (var asset in enumerable)
            {
                asset.Category = category ?? asset.Category;
            }
            var target = AllAssets;
            if (target != null)
            {
                Assets?.Refresh();
                await _catalogService.SaveTo(AssetsCatalogPath, target.Select(i => i.Item).ToList());
            }
        }

        internal async Task RemoveAssetsAsync(IEnumerable<AssetViewModel> enumerable)
        {
            var target = AllAssets;
            if (target != null)
            {
                AllAssets?.RemoveRange(enumerable);
                await _catalogService.SaveTo(AssetsCatalogPath, target.Select(i => i.Item).ToList());
            }
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

        private string filterModId = string.Empty;
        public string FilterModId
        {
            get { return filterModId; }
            set
            {
                if (filterModId != value)
                {
                    filterModId = value;
                    Assets?.Refresh();
                }
                NotifyOfPropertyChange();
            }
        }

        private AssetCatalogCategory? filterCategory = null;
        public AssetCatalogCategory? FilterCategory
        {
            get { return filterCategory; }
            set
            {
                if (filterCategory != value)
                {
                    filterCategory = value;
                    Assets?.Refresh();
                }
                NotifyOfPropertyChange();
            }
        }

        public ICommand ImportA3 { get; }
        public ICommand ImportUsualMod { get; }
        public ICommand ImportActiveMod { get; }

        public bool IsImporting { get; set; }
    }
}
