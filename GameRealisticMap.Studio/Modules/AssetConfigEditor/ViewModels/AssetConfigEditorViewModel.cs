using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Arma3.Assets.Rows;
using GameRealisticMap.Arma3.GameLauncher;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.Arma3Data.ViewModels;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Railways;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Roads;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Rows;
using GameRealisticMap.Studio.Modules.CompositionTool;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Modules.Main;
using GameRealisticMap.Studio.Modules.Main.Services;
using GameRealisticMap.Studio.Modules.Reporting;
using GameRealisticMap.Studio.Toolkit;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;
using Gemini.Framework.Services;
using NLog;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class AssetConfigEditorViewModel : PersistedDocument, IExplorerRootTreeItem, IMainDocument
    {
        private static readonly Logger logger = NLog.LogManager.GetLogger("Arma3AssetConfigEditor");

        private readonly IArma3DataModule _arma3Data;
        private readonly IShell _shell;
        private readonly ICompositionTool _compositionTool;

        public BindableCollection<IFillAssetCategory> Filling { get; } = new BindableCollection<IFillAssetCategory>();

        public BindableCollection<FencesViewModel> Fences { get; } = new BindableCollection<FencesViewModel>();

        public BindableCollection<BuildingsViewModel> Buildings { get; } = new BindableCollection<BuildingsViewModel>();

        public BindableCollection<NaturalRowViewModel> NaturalRows { get; } = new BindableCollection<NaturalRowViewModel>();

        public BindableCollection<ObjectsViewModel> Objects { get; } = new BindableCollection<ObjectsViewModel>();

        public BindableCollection<MaterialViewModel> Materials { get; } = new BindableCollection<MaterialViewModel>();

        public BindableCollection<RoadViewModel> Roads { get; } = new BindableCollection<RoadViewModel>();

        public BindableCollection<PondViewModel> Ponds { get; } = new BindableCollection<PondViewModel>();

        public BindableCollection<IAssetCategory> Railways { get; } = new BindableCollection<IAssetCategory>();

        public BindableCollection<SidewalkViewModel> Sidewalks { get; } = new BindableCollection<SidewalkViewModel>();

        public List<ICommandWithLabel> AdditionalFilling { get; }

        public List<ICommandWithLabel> AdditionalFences { get; }

        public List<ICommandWithLabel> AdditionalNaturalRows { get; }

        public ICommandWithLabel AdditionalSidewalks { get; }

        public RelayCommand RemoveFilling { get; }

        public RelayCommand RemoveFence { get; }

        public RelayCommand RemoveNaturalRow { get; }

        public RelayCommand RemoveSidewalks { get; }

        public bool IsLoading { get; set; }

        public List<ImportConfigCommand> BuiltinAssetConfigFiles { get; }

        bool canCopyFrom;
        public bool CanCopyFrom { get => canCopyFrom; set { if (canCopyFrom != value) { canCopyFrom = value; NotifyOfPropertyChange(); } } }

        public IArma3DataModule Arma3DataModule => _arma3Data;

        public AssetConfigEditorViewModel(IArma3DataModule arma3Data, IShell shell, ICompositionTool compositionTool)
        {
            _arma3Data = arma3Data;
            _shell = shell;
            _compositionTool = compositionTool;
            Children = new List<IExplorerTreeItem>()
            {
                new ExplorerTreeItem(Labels.NaturalAreas, Filling, "Nature"),
                new ExplorerTreeItem(Labels.FencesWalls, Fences, "Fence"),
                new ExplorerTreeItem(Labels.NaturalRows, NaturalRows, "NaturalRows"),
                new ExplorerTreeItem(Labels.Buildings, Buildings, "Buildings"),
                new ExplorerTreeItem(Labels.AssetObjects, Objects, "Objects"),
                new ExplorerTreeItem(Labels.GroundMaterials, Materials, "Materials"),
                new ExplorerTreeItem(Labels.RoadsAndBridges, Roads, "Road"),
                new ExplorerTreeItem(Labels.Railways, Railways, "Railways"),
                new ExplorerTreeItem("Sidewalks", Sidewalks, "Sidewalks")
            };
            UndoRedoManager.PropertyChanged += (_, _) => { IsDirty = true; CanCopyFrom = false; };
            AdditionalFilling = CreateNatureFilling();
            AdditionalFences = CreateFences();
            AdditionalNaturalRows = CreateAdditional<NaturalRowType,NaturalRowViewModel>(i => new NaturalRowViewModel(i, null, this), NaturalRows);
            RemoveFilling = new RelayCommand(item => DoRemoveFilling((IFillAssetCategory)item, Filling));
            RemoveFence = new RelayCommand(item => DoRemoveFilling((FencesViewModel)item, Fences));
            RemoveNaturalRow = new RelayCommand(item => DoRemoveFilling((NaturalRowViewModel)item, NaturalRows));
            RemoveSidewalks = new RelayCommand(item => DoRemoveFilling((SidewalkViewModel)item, Sidewalks));
            BuiltinAssetConfigFiles = Arma3Assets.GetBuiltinList().Select(builtin => new ImportConfigCommand(builtin, this)).ToList();
            AdditionalSidewalks = CreateAdditional<SidewalkId, SidewalkViewModel>(id => new SidewalkViewModel(null, this), Sidewalks).First();
        }

        internal List<ICommandWithLabel> CreateNatureFilling()
        {
            var list = new List<ICommandWithLabel>();
            list.AddRange(CreateAdditional<BasicCollectionId, IFillAssetCategory>(id => new FillingAssetBasicViewModel(id, null, this), Filling));
            list.AddRange(CreateAdditional<ClusterCollectionId, IFillAssetCategory>(id => new FillingAssetClusterViewModel(id, null, this), Filling));
            return SortedByLabel(list);
        }

        internal List<ICommandWithLabel> CreateFences()
        {
            return SortedByLabel(CreateAdditional<FenceTypeId, FencesViewModel>(id => new FencesViewModel(id, null, this), Fences));
        }

        internal List<ICommandWithLabel> CreateAdditional<TEnum,TViewModel>(Func<TEnum,TViewModel> create, BindableCollection<TViewModel> target)
            where TEnum : struct, Enum
            where TViewModel : class, IFillAssetCategory
        {
            var list = new List<ICommandWithLabel>();
            foreach (var id in Enum.GetValues<TEnum>())
            {
                list.Add(new AdditionalFilling<TViewModel>(id.ToString(), () => create(id), target, UndoRedoManager));
            }
            return list;
        }

        private static List<ICommandWithLabel> SortedByLabel(List<ICommandWithLabel> list)
        {
            list.Sort((a, b) => a.Label.CompareTo(b.Label));
            return list;
        }

        private void DoRemoveFilling<T>(T item, BindableCollection<T> list) 
            where T : class, IFillAssetCategory
        {
            if (list.Count(f => f != item && f.IsSameFillId(item.IdObj)) >= 1)
            {
                list.RemoveUndoable(UndoRedoManager, item);
            }
        }

        public double TextureSizeInMeters { get; set; }

        public IEnumerable<IExplorerTreeItem> Children { get; }

        public string TreeName => DisplayName;
        public string Icon => $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/AssetConfig.png";

        public override string DisplayName { get => base.DisplayName; set { base.DisplayName = value; NotifyOfPropertyChange(nameof(TreeName)); } }

        protected override async Task DoLoad(string filePath)
        {
            _ = Task.Run(() => DoLoadBackground(filePath));

            await IoC.Get<IRecentFilesService>().AddRecentFile(filePath);
        }

        private async Task DoLoadBackground(string filePath)
        {
            IsLoading = true;
            NotifyOfPropertyChange(nameof(IsLoading));

            try
            {
                Arma3Assets json;
                try
                {
                    json = await Arma3Assets.LoadFromFile(_arma3Data.Library, filePath);
                }
                catch (ApplicationException)
                {
                    json = await AutoEnableMods(filePath);
                }
                await FromJson(json);

                await _arma3Data.SaveLibraryCache();

                IsLoading = false;
                NotifyOfPropertyChange(nameof(IsLoading));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to read '{0}'", filePath);
                OnUIThread(() => App.ShowException(ex));
            }
        }

        private async Task<Arma3Assets> AutoEnableMods(string filePath)
        {
            var deps = await Arma3Assets.LoadDependenciesFromFile(filePath);
            MissingMods = await MissingMod.DetectMissingMods(_arma3Data, IoC.Get<IArma3ModsService>(), deps);
            NotifyOfPropertyChange(nameof(HasMissingMods));
            NotifyOfPropertyChange(nameof(MissingMods));
            try
            {
                // Last attempt
                return await Arma3Assets.LoadFromFile(_arma3Data.Library, filePath);
            }
            catch (ApplicationException)
            {
                return await Arma3Assets.LoadFromFile(_arma3Data.Library, filePath, true);
            }
        }

        internal Task CopyFrom(string filePath)
        {
            CanCopyFrom = false;
            return DoLoad(filePath);
        }

        private async Task FromJson(Arma3Assets arma3Assets)
        {
            Filling.Clear();
            Fences.Clear();
            Buildings.Clear();
            Objects.Clear();
            Roads.Clear();
            Materials.Clear();
            Ponds.Clear();
            Railways.Clear();
            NaturalRows.Clear();
            Sidewalks.Clear();

            Filling.AddRange(GetFilling(arma3Assets));
            Fences.AddRange(GetFences(arma3Assets));
            Objects.AddRange(GetObjects(arma3Assets));
            Buildings.AddRange(GetBuildings(arma3Assets));
            NaturalRows.AddRange(GetNaturalRows(arma3Assets));
            Sidewalks.AddRange(arma3Assets.Sidewalks.Select(e => new SidewalkViewModel(e, this)));
            if (Sidewalks.Count ==0)
            {
                Sidewalks.Add(new SidewalkViewModel(null, this));
            }
            foreach (var id in Enum.GetValues<TerrainMaterialUsage>().OrderByDescending(i => i))
            {
                Materials.Add(await MaterialViewModel.Create(id, arma3Assets, this));
            }
            TextureSizeInMeters = arma3Assets.Materials.TextureSizeInMeters;
            foreach (var id in Enum.GetValues<RoadTypeId>().OrderByDescending(i => i))
            {
                Roads.Add(new RoadViewModel(id, arma3Assets.Roads.FirstOrDefault(m => m.Id == id), arma3Assets.GetBridge(id), this));
            }
            foreach (var id in Enum.GetValues<PondSizeId>())
            {
                Ponds.Add(new PondViewModel(id, arma3Assets.Ponds.Count == 0 ? null : arma3Assets.GetPond(id), _arma3Data.Library));
            }
            NotifyOfPropertyChange(nameof(TextureSizeInMeters));
            BaseWorldName = arma3Assets.BaseWorldName;
            BaseDependency = arma3Assets.BaseDependency;
            Railways.Add(new RailwaysStraightViewModel(arma3Assets.Railways?.Straights, this));
            Railways.Add(new RailwaysCrossingViewModel(arma3Assets.Railways?.Crossings, this));
        }

        private List<Individual.ObjectsViewModel> GetObjects(Arma3Assets arma3Assets)
        {
            var list = new List<Individual.ObjectsViewModel>();
            foreach (var id in Enum.GetValues<ObjectTypeId>())
            {
                list.Add(new Individual.ObjectsViewModel(id, arma3Assets.GetObjects(id), this));
            }
            list.Sort((a, b) => a.PageTitle.CompareTo(b.PageTitle));
            return list;
        }

        private List<BuildingsViewModel> GetBuildings(Arma3Assets arma3Assets)
        {
            var list = new List<BuildingsViewModel>();
            foreach (var id in Enum.GetValues<BuildingTypeId>())
            {
                list.Add(new BuildingsViewModel(id, arma3Assets.GetBuildings(id), this));
            }
            list.Sort((a, b) => a.PageTitle.CompareTo(b.PageTitle));
            return list;
        }

        private List<IFillAssetCategory> GetFilling(Arma3Assets arma3Assets)
        {
            var list = new List<IFillAssetCategory>();
            foreach (var id in Enum.GetValues<BasicCollectionId>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetBasicCollections(id)))
                {
                    list.Add(new FillingAssetBasicViewModel(id, entry, this));
                }
            }
            foreach (var id in Enum.GetValues<ClusterCollectionId>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetClusterCollections(id)))
                {
                    list.Add(new FillingAssetClusterViewModel(id, entry, this));
                }
            }
            list.Sort((a, b) => a.PageTitle.CompareTo(b.PageTitle));
            return list;
        }

        private List<FencesViewModel> GetFences(Arma3Assets arma3Assets)
        {
            var list = new List<FencesViewModel>();
            foreach (var id in Enum.GetValues<FenceTypeId>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetFences(id)))
                {
                    list.Add(new FencesViewModel(id, entry, this));
                }
            }
            list.Sort((a, b) => a.PageTitle.CompareTo(b.PageTitle));
            return list;
        }

        private List<NaturalRowViewModel> GetNaturalRows(Arma3Assets arma3Assets)
        {
            var list = new List<NaturalRowViewModel>();
            foreach (var id in Enum.GetValues<NaturalRowType>())
            {
                foreach (var entry in AtLeastOne(arma3Assets.GetNaturalRows(id)))
                {
                    list.Add(new NaturalRowViewModel(id, entry, this));
                }
            }
            list.Sort((a, b) => a.PageTitle.CompareTo(b.PageTitle));
            return list;
        }

        private IEnumerable<T?> AtLeastOne<T>(IReadOnlyCollection<T> collection, T? one = null) where T : class
        {
            if (collection.Count == 0)
            {
                return new T?[] { one };
            }
            return collection;
        }

        internal Arma3Assets ToJson()
        {
            EquilibrateProbabilities();
            var json = new Arma3Assets();
            json.ClusterCollections = Filling.OfType<FillingAssetClusterViewModel>().Where(c => !c.IsEmpty).GroupBy(c => c.FillId).OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());
            json.BasicCollections = Filling.OfType<FillingAssetBasicViewModel>().Where(c => !c.IsEmpty).GroupBy(c => c.FillId).OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());
            json.Fences = Fences.OfType<FencesViewModel>().Where(c => !c.IsEmpty).GroupBy(c => c.FillId).OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());
            json.Buildings = Buildings.Where(c => !c.IsEmpty).OrderBy(k => k.FillId).ToDictionary(k => k.FillId, k => k.ToDefinition());
            json.Objects = Objects.Where(c => !c.IsEmpty).OrderBy(k => k.FillId).ToDictionary(k => k.FillId, k => k.ToDefinition());
            json.NaturalRows = NaturalRows.Where(c => !c.IsEmpty).GroupBy(c => c.FillId).OrderBy(k => k.Key).ToDictionary(k => k.Key, k => k.Select(o => o.ToDefinition()).ToList());

            var materialDefintions = Materials
                .GroupBy(m => new { m.ActualColorId, m.ColorTexture, m.NormalTexture })
                .Select(m => new TerrainMaterialDefinition(
                    m.First().ToDefinition(),
                    m.Select(o => o.FillId).OrderBy(m => m).ToArray(),
                    m.First().GetSurfaceConfig(),
                    m.First().GetData()))
                .OrderBy(m => m.Usages.Min())
                .ToList();

            json.Materials = new TerrainMaterialLibrary(materialDefintions, TextureSizeInMeters);
            json.Roads = Roads.OrderBy(k => k.FillId).Select(r => r.ToDefinition()).ToList();
            json.Bridges = Roads.Select(r => new { Key = r.FillId, Bridge = r.ToBridgeDefinition() })
                .Where(b => b.Bridge != null)
                .OrderBy(k => k.Key)
                .ToDictionary(k => k.Key, k => k.Bridge!);
            json.Ponds = Ponds.ToDictionary(p => p.Id, k => k.ToDefinition());
            json.Railways = new RailwaysDefinition(
                Railways.OfType<RailwaysStraightViewModel>().FirstOrDefault()?.ToDefinition() ?? new List<StraightSegmentDefinition>(),
                Railways.OfType<RailwaysCrossingViewModel>().FirstOrDefault()?.ToDefinition() ?? new List<RailwayCrossingDefinition>()
                );
            json.BaseDependency = baseDependency;
            json.BaseWorldName = baseWorldName;
            json.Sidewalks = Sidewalks.Where(e => !e.IsEmpty).Select(e => e.ToDefinition()).ToList();
            json.Dependencies = ComputeModDependencies();
            return json;
        }

        private void EquilibrateProbabilities()
        {
            foreach (var list in Filling.OfType<FillingAssetClusterViewModel>().GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.Where(i => !i.IsEmpty).ToList());
            }
            foreach (var list in Filling.OfType<FillingAssetBasicViewModel>().GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.Where(i => !i.IsEmpty).ToList());
            }
            foreach (var list in Fences.OfType<FencesViewModel>().GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.Where(i => !i.IsEmpty).ToList());
            }
            foreach (var list in NaturalRows.GroupBy(c => c.FillId))
            {
                DefinitionHelper.EquilibrateProbabilities(list.Where(i => !i.IsEmpty).ToList());
            }
            DefinitionHelper.EquilibrateProbabilities(Sidewalks.Where(e => !e.IsEmpty).ToList());
            foreach (var item in Filling.Concat(Fences).Concat(NaturalRows).Concat(Buildings.Cast<IAssetCategory>()).Concat(Objects))
            {
                item.Equilibrate();
            }
        }

        protected override async Task DoNew()
        {
            await FromJson(new Arma3Assets());
            CanCopyFrom = true;
        }

        protected override async Task DoSave(string filePath)
        {
            try
            {
                var json = ToJson();
                using var stream = File.Create(filePath);
                await SaveTo(stream, json).ConfigureAwait(false);
                await IoC.Get<IRecentFilesService>().AddRecentFile(filePath);
                CanCopyFrom = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to write '{0}'", filePath);
                App.ShowException(ex);
            }
        }

        internal Task EditAssetCategory(IDocument document)
        {
            return _shell.OpenDocumentAsync(document);
        }

        internal void EditComposition(IWithComposition document)
        {
            _shell.ShowTool(_compositionTool);
            _compositionTool.Current = document;
            _compositionTool.UndoRedoManager = UndoRedoManager;
        }

        private string baseWorldName = string.Empty;
        public string BaseWorldName
        {
            get { return baseWorldName; }
            set { baseWorldName = value; NotifyOfPropertyChange(); }
        }

        private string baseDependency = string.Empty;
        public string BaseDependency
        {
            get { return baseDependency; }
            set { baseDependency = value; NotifyOfPropertyChange(); }
        }

        public bool HasMissingMods => MissingMods.Count > 0;

        public List<MissingMod> MissingMods { get; private set; } = new List<MissingMod>();

        public IEnumerable<string> ListReferencedFiles()
        {
            return Filling.SelectMany(f => f.GetModels())
                .Concat(Fences.SelectMany(f => f.GetModels()))
                .Concat(Buildings.SelectMany(f => f.GetModels()))
                .Concat(Objects.SelectMany(f => f.GetModels()))
                .Concat(Roads.SelectMany(f => f.GetModels()))
                .Concat(Roads.SelectMany(f => f.GetTextures()))
                .Concat(Ponds.Where(p => !string.IsNullOrEmpty(p.Model)).Select(p => p.Model!))
                .Concat(Railways.SelectMany(f => f.GetModels()))
                .Concat(NaturalRows.SelectMany(f => f.GetModels()))
                .Concat(Sidewalks.SelectMany(f => f.GetModels()))
                .Concat(Materials.SelectMany(f => f.GetModels()))
                .Concat(Materials.SelectMany(f => f.GetTextures()))
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> ListReferencedModelsButNotClutter()
        {
            return Filling.SelectMany(f => f.GetModels())
                .Concat(Fences.SelectMany(f => f.GetModels()))
                .Concat(Buildings.SelectMany(f => f.GetModels()))
                .Concat(Objects.SelectMany(f => f.GetModels()))
                .Concat(Roads.SelectMany(f => f.GetModels()))
                .Concat(Ponds.Where(p => !string.IsNullOrEmpty(p.Model)).Select(p => p.Model!))
                .Concat(Railways.SelectMany(f => f.GetModels()))
                .Concat(NaturalRows.SelectMany(f => f.GetModels()))
                .Concat(Sidewalks.SelectMany(f => f.GetModels()))
                .Where(f => !string.IsNullOrEmpty(f))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        public List<ModDependencyDefinition> ComputeModDependencies()
        {
            return IoC.Get<IArma3Dependencies>().ComputeModDependencies(ListReferencedFiles()).ToList();
        }

        public async Task Reload()
        {
            if (!IsNew)
            {
                MissingMods.Clear();
                NotifyOfPropertyChange(nameof(HasMissingMods));
                NotifyOfPropertyChange(nameof(MissingMods));

                await DoLoad(FilePath);
            }
        }

        public Task GenerateDemoMod()
        {
            Arma3ToolsHelper.EnsureProjectDrive();

            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateDemoMod, DoGenerateMod);

            return Task.CompletedTask;
        }

        public Task GenerateDemoWrp()
        {
            IoC.Get<IProgressTool>()
                .RunTask(Labels.GenerateDemoWrpFile, DoGenerateWrp);

            return Task.CompletedTask;
        }

        private async Task DoGenerateMod(IProgressTaskUI task)
        {
            var name = Path.GetFileNameWithoutExtension(FileName);
            var assets = ToJson();
            var generator = new Arma3DemoMapGenerator(assets, _arma3Data.ProjectDrive, name, _arma3Data.CreatePboCompilerFactory(), new StudioDemoNaming());
            var config = await generator.GenerateMod(task.Scope);
            if (config != null)
            {
                task.AddSuccessAction(() => ShellHelper.OpenUri(config.TargetModDirectory), Labels.ViewInFileExplorer);
                task.AddSuccessAction(() => ShellHelper.OpenUri("steam://run/107410"), Labels.OpenArma3Launcher, string.Format(Labels.OpenArma3LauncherWithGeneratedModHint, name));
                task.AddSuccessAction(() => Arma3Helper.Launch(assets.Dependencies, config.TargetModDirectory, config.WorldName), Labels.LaunchArma3, Labels.LaunchArma3Hint);
                await Arma3LauncherHelper.CreateLauncherPresetAsync(assets.Dependencies, config.TargetModDirectory, "GRM - " + name);
            }
        }

        private async Task DoGenerateWrp(IProgressTaskUI task)
        {
            var name = Path.GetFileNameWithoutExtension(FileName);
            var assets = ToJson();
            var generator = new Arma3DemoMapGenerator(assets, _arma3Data.ProjectDrive, name, _arma3Data.CreatePboCompilerFactory(), new StudioDemoNaming());
            var config = await generator.GenerateWrp(task.Scope);
            if (config != null)
            {
                task.AddSuccessAction(() => ShellHelper.OpenUri(_arma3Data.ProjectDrive.GetFullPath(config.PboPrefix)), Labels.ViewInFileExplorer);;
            }
        }

        public async Task SaveTo(Stream stream)
        {
            await SaveTo(stream, ToJson()).ConfigureAwait(false);
        }

        public async Task SaveTo(Stream stream, Arma3Assets json)
        {
            await JsonSerializer.SerializeAsync(stream, ToJson(), Arma3Assets.CreateJsonSerializerOptions(_arma3Data.Library)).ConfigureAwait(false);
        }

        public Task TakeAerialImages()
        {
            return IoC.Get<IWindowManager>().ShowDialogAsync(new Arma3AerialImageViewModel(ListReferencedModelsButNotClutter().ToList(), ComputeModDependencies()));
        }

    }
}
