﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Demo;
using GameRealisticMap.Arma3.GameEngine.Materials;
using GameRealisticMap.Arma3.GameLauncher;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;
using NLog;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels
{
    [Export(typeof(GdtBrowserViewModel))]
    internal class GdtBrowserViewModel : BrowserViewModelBase, IPersistedDocument
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("GdtBrowser");

        private readonly IArma3Previews _arma3Previews;
        private readonly IGdtCatalogStorage _catalogService;
        private readonly Task<BindableCollection<GdtDetailViewModel>> awaitableItems;
        private bool isSaving;
        private bool _isDirty;

        [ImportingConstructor]
        public GdtBrowserViewModel(IArma3DataModule arma3DataModule, IArma3Previews arma3Previews, IGdtCatalogStorage catalogService, IArma3ModsService arma3ModsService)
            : base(arma3DataModule, arma3ModsService)
        {
            AllItems = new BindableCollection<GdtDetailViewModel>();

            _arma3Previews = arma3Previews;
            _catalogService = catalogService;
            _catalogService.Updated += CatalogServiceUpdated;
            UpdateDisplayName();
            awaitableItems = Task.Run(DoLoad);
            UndoRedoManager.PropertyChanged += (_, _) => { IsDirty = true; }; 
        }

        public BindableCollection<GdtDetailViewModel> AllItems { get; private set; }

        public ICollectionView? Items { get; private set; }

        public IArma3Previews Previews => _arma3Previews;

        public bool IsNew => false;

        public string FileName => string.Empty;

        public string FilePath => string.Empty;
        public bool IsDirty { get { return _isDirty; } set { if (Set(ref _isDirty, value)) { UpdateDisplayName(); } } }
        
        private void UpdateDisplayName()
        {
            DisplayName = IsDirty ? Labels.GroundDetailTextureLibrary + " - Arma 3*" : Labels.GroundDetailTextureLibrary + " - Arma 3";
        }

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

            ComputeColorUniqueness();

            IsImporting = false;

            return AllItems;
        }

        public Task ImportA3()
        {
            Task.Run(async () =>
            {
                IsImporting = true;
                await SaveChanges();
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

        public async Task<GdtDetailViewModel?> Create()
        {
            var createTextureVm = new GdtCreateViewModel(this);

            if ((await IoC.Get<IWindowManager>().ShowDialogAsync(createTextureVm)) ?? false)
            {
                return createTextureVm.Result;
            }

            return null;
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
                await SaveChanges();
                await _catalogService.ImportMod(installed);
                IsImporting = false;
            });
        }

        internal async Task SaveChanges()
        {
            isSaving = true;
            await _catalogService.SaveChanges(AllItems.Select(i => i.ToDefinition()).ToList());
            foreach(var item in AllItems)
            {
                item.OnSave();
            }
            isSaving = false;
            IsDirty = false;
            IoC.Get<IArma3ImageStorage>().ProcessPngToPaaBackground();
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

        internal async Task<GdtDetailViewModel> ImportExternal(TerrainMaterial terrainMaterial, SurfaceConfig? surfaceConfig, string? title = null)
        {
            var allItems = (await awaitableItems);
            var vm = new GdtDetailViewModel(this, new GdtCatalogItem(terrainMaterial, surfaceConfig, GdtCatalogItemType.GameData, title));
            if (allItems.Any(i => i.ColorId == vm.ColorId))
            {
                // Ensure uniqueness
                vm.ColorId = GdtHelper.AllocateUniqueColor(vm.ColorId, allItems.Select(i => i.ColorId));
            }
            allItems.Add(vm);
            return vm;
        }

        internal async Task<GdtDetailViewModel?> ImportExternal(string colorTexture, string normalTexture)
        {
            using var fakeSat = GdtHelper.GenerateFakeSatPngImage(_arma3Previews, colorTexture);
            if (fakeSat != null)
            {
                var allItems = (await awaitableItems);
                var color = GdtHelper.AllocateUniqueColor(fakeSat, allItems.Select(i => i.ColorId));
                var config = new GdtCatalogItem(new TerrainMaterial(normalTexture, colorTexture, color.ToRgb24(), fakeSat?.ToPngByteArray()), null, GdtCatalogItemType.GameData, null);
                var vm = new GdtDetailViewModel(this, config);
                allItems.Add(vm);
                return vm;
            }
            return null;
        }

        internal async Task<GdtDetailViewModel> Resolve(TerrainMaterial mat, SurfaceConfig? surf, string? title = null)
        {
            var items = await awaitableItems;
            var libraryItem = items.FirstOrDefault(i => string.Equals(i.ColorTexture, mat.ColorTexture, StringComparison.OrdinalIgnoreCase));
            if (libraryItem == null)
            {
                libraryItem = await ImportExternal(mat, surf, title);
            }
            return libraryItem;
        }

        internal async Task<GdtDetailViewModel?> Resolve(string colorTexture)
        {
            return (await awaitableItems).FirstOrDefault(i => string.Equals(i.ColorTexture, colorTexture, StringComparison.OrdinalIgnoreCase));
        }

        internal void ComputeColorUniqueness()
        {
            foreach(var grp in AllItems.GroupBy(i => i.ColorId))
            {
                var isNotUnique = grp.Count() != 1;
                foreach(var entry in grp)
                {
                    entry.IsNotColorUnique = isNotUnique;
                }
            }
        }

        public Task GenerateDemoMap()
        {
            return GenerateDemoMap(AllItems);
        }

        public Task GenerateDemoMap(IEnumerable<GdtDetailViewModel> items)
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateDemoMod, task => DoGenerateMod(task, items));

            return Task.CompletedTask;
        }

        public Task New(string fileName)
        {
            throw new NotSupportedException();
        }

        public Task Load(string filePath)
        {
            throw new NotSupportedException();
        }

        public Task Save(string filePath)
        {
            return SaveChanges();
        }

        private async Task DoGenerateMod(IProgressTaskUI task, IEnumerable<GdtDetailViewModel> items)
        {
            var dependencies = 
                IoC.Get<IArma3Dependencies>()
                .ComputeModDependencies(
                    items.SelectMany(i => i.GetModels())
                    .Concat(items.SelectMany(i => i.GetTextures()))
                    .Where(f => !string.IsNullOrEmpty(f))
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                .ToList();

            var name = DisplayName;

            var generator = new Arma3GdtDemoMapGenerator(items.OrderBy(a => a.DisplayName).Select(a => new TerrainMaterialDefinition(
                a.ToMaterial(),
                new TerrainMaterialUsage[0], 
                a.ToSurfaceConfig(), 
                a.ToData(),
                a.Title)), _arma3DataModule.ProjectDrive, _arma3DataModule.CreatePboCompilerFactory());

            var config = await generator.GenerateMod(task.Scope);
            if (config != null)
            {
                task.AddSuccessAction(() => ShellHelper.OpenUri(config.TargetModDirectory), Labels.ViewInFileExplorer);
                task.AddSuccessAction(() => ShellHelper.OpenUri("steam://run/107410"), Labels.OpenArma3Launcher, string.Format(Labels.OpenArma3LauncherWithGeneratedModHint, name));
                task.AddSuccessAction(() => Arma3Helper.Launch(dependencies, config.TargetModDirectory, config.WorldName), Labels.LaunchArma3, Labels.LaunchArma3Hint);
                await Arma3LauncherHelper.CreateLauncherPresetAsync(dependencies, config.TargetModDirectory, "GRM - " + name);
            }
        }

        internal async Task<GdtDetailViewModel> Add(GdtCatalogItem config)
        {
            var result = new GdtDetailViewModel(this, config);
            AllItems.AddUndoable(UndoRedoManager, result);
            await result.OpenMaterial();
            return result;
        }

        internal async Task Remove(GdtDetailViewModel gdtDetailViewModel)
        {
            await gdtDetailViewModel.TryCloseAsync();
            AllItems.RemoveUndoable(UndoRedoManager, gdtDetailViewModel);
        }


        internal async Task<TerrainMaterialLibrary> ToTerrainMaterialLibrary()
        {
            var allItems = (await awaitableItems);
            return new TerrainMaterialLibrary(allItems.Select(i => new TerrainMaterialDefinition(i.ToMaterial(), new TerrainMaterialUsage[0], i.ToSurfaceConfig(), i.ToData())).ToList());
        }

        internal async Task<TerrainMaterialLibrary> ToTerrainMaterialLibraryIdOnly()
        {
            var allItems = (await awaitableItems);
            return new TerrainMaterialLibrary(allItems.Select(i => new TerrainMaterialDefinition(i.ToMaterial(), new TerrainMaterialUsage[0])).ToList());
        }
    }
}
