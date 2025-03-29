using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using BIS.WRP;
using Caliburn.Micro;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Modules.Inspector;
using Gemini.Modules.Shell.Commands;
using Pmad.HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class Arma3WorldMapViewModel : Document,
        ICommandHandler<SaveFileCommandDefinition>,
        ICommandHandler<SaveFileAsCommandDefinition>
    {
        private readonly Arma3WorldEditorViewModel parentEditor;
        private readonly IArma3DataModule arma3Data;
        private readonly IInspectorTool inspectorTool;
        private IEditablePointCollection? editPoints;

        private BackgroundMode _backgroundMode;
        private EditTool _editTool;
        private GrmMapEditMode _editMode;
        private EditableArma3RoadTypeInfos? selectectedRoadType;
        private IEnumerable<IEditablePointCollection>? selectedItems;
        private readonly Dictionary<EditableArma3Road, EditRoadEditablePointCollection> cache = new Dictionary<EditableArma3Road, EditRoadEditablePointCollection>();
        private readonly Dictionary<EditableWrpObject, TerrainObjectVM> objCache = new Dictionary<EditableWrpObject, TerrainObjectVM>();
        private List<EditableWrpObject>? initialList = null;

        public Arma3WorldMapViewModel(Arma3WorldEditorViewModel parent, IArma3DataModule arma3Data)
        {
            this.parentEditor = parent;
            this.arma3Data = arma3Data;
            this.inspectorTool = IoC.Get<IInspectorTool>();
            DisplayName = parent.DisplayName + " - Editor";
        }

        public bool CanMerge => selectedItems != null 
            && selectedItems.All(i => i is EditRoadEditablePointCollection)
            && CanMergeRoads(selectedItems.Cast<EditRoadEditablePointCollection>().ToList());

        private static bool CanMergeRoads(List<EditRoadEditablePointCollection> editRoadEditablePointCollections)
        {
            return editRoadEditablePointCollections.Count == 2 && editRoadEditablePointCollections[0].RoadTypeInfos == editRoadEditablePointCollections[1].RoadTypeInfos;
        }

        public IEditablePointCollection? EditPoints
        {
            get { return editPoints; }
            set 
            { 
                if (editPoints != value) 
                {
                    editPoints = value; 
                    NotifyOfPropertyChange(); 
                    inspectorTool.SelectedObject = value as IInspectableObject;

                    if (editPoints != null && selectedItems != null)
                    {
                        SelectedItems = null;
                    }
                } 
            }
        }
        public IEnumerable<IEditablePointCollection>? SelectedItems
        {
            get { return selectedItems; }
            set
            {
                if (selectedItems != value)
                {
                    selectedItems = value;
                    NotifyOfPropertyChange();

                    if (editPoints != null && selectedItems != null)
                    {
                        EditPoints = null;
                    }
                    if (selectedItems != null && selectedItems.Count() == 1)
                    {
                        inspectorTool.SelectedObject = selectedItems.First() as IInspectableObject;
                    }
                    NotifyOfPropertyChange(nameof(CanMerge));
                }
            }
        }

        public TerrainSpacialIndex<TerrainObjectVM>? Objects { get; set; }

        public ICommand SelectItemCommand => new RelayCommand(SelectItem, _ => GrmMapEditMode == GrmMapEditMode.None);

        public ICommand AddToSelectionCommand => new RelayCommand(AddToSelection);

        public ICommand InsertPointCommand => new RelayCommand(p => InsertPoint((TerrainPoint)p));

        public ICommand DeleteSelectionCommand => new RelayCommand(_ => DeleteSelection());

        public ICommand MergeSelectionCommand => new RelayCommand(_ => MergeSelection());

        private void MergeSelection()
        {
            if (selectedItems != null)
            {
                var items = selectedItems.Cast<EditRoadEditablePointCollection>().ToList();
                if (CanMergeRoads(items))
                {
                    UndoRedoManager.ExecuteAction(new MergeRoadAction(this, items[0], items[1]));
                }
            }
        }

        private void DeleteSelection()
        {
            if (selectedItems != null)
            {
                foreach (var road in selectedItems.OfType<IEditablePointCollection>())
                {
                    road.Remove();
                }
                SelectedItems = null;
            }
            else if (editPoints != null)
            {
                editPoints.Remove();
                EditPoints = null;
            }
        }

        private void InsertPoint(TerrainPoint point)
        {
            var roads = Roads;
            if (roads != null && selectectedRoadType != null)
            {
                UndoRedoManager.ExecuteAction(new AddRoadAction(this, roads, selectectedRoadType, point));
                _editMode = GrmMapEditMode.ContinuePath;
                NotifyOfPropertyChange(nameof(GrmMapEditMode));
            }
        }

        internal EditRoadEditablePointCollection CreateEdit(EditableArma3Road road)
        {
            if (!cache.TryGetValue(road, out var edit))
            {
                edit = new EditRoadEditablePointCollection(road, this);
                edit.CollectionChanged += MakeRoadsDirty;
                cache[road] = edit;
            }
            return edit;
        }


        public void SelectItem(object? item)
        {
            if (GrmMapEditMode == GrmMapEditMode.None)
            {
                Select(GetEditable(item));
            }
        }

        private IEditablePointCollection? GetEditable(object? item)
        {
            if (item is EditableArma3Road road)
            {
                return CreateEdit(road);
            }
            return item as IEditablePointCollection;
        }

        public void AddToSelection(object? item)
        {
            var editable = GetEditable(item);
            if (editable != null)
            {
                if (selectedItems != null)
                {
                    SelectedItems = selectedItems.Concat(new[] { editable }).Distinct().ToList();
                }
                else if (editPoints != null)
                {
                    SelectedItems = new[] { editPoints, editable }.Distinct().ToList();
                }
                else
                {
                    Select(editable);
                }
            }
        }

        private void Select(IEditablePointCollection? editable)
        {
            if (editable is EditRoadEditablePointCollection)
            {
                SelectedItems = null;
                EditPoints = editable;
            }
            else if (editable != null)
            {
                EditPoints = editable;
                // In current version, we do not allow to move the object, so switch to outline mode
                SelectedItems = new List<IEditablePointCollection> { editable };
            }
            else
            {
                SelectedItems = null;
                EditPoints = null;
            }
        }

        private void MakeRoadsDirty(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            parentEditor.IsRoadsDirty = true;
        }

        public EditableArma3Roads? Roads => parentEditor.Roads;

        public float SizeInMeters => parentEditor.SizeInMeters ?? 2500;

        public HugeImage<Rgb24>? SatMap { get; set; }

        public HugeImage<Rgb24>? IdMap { get; set; }

        public HugeImage<Rgb24>? BackgroundImage
        {
            get
            {
                switch (BackgroundMode)
                {
                    case BackgroundMode.Satellite: return SatMap;
                    case BackgroundMode.TextureMask: return IdMap;
                    default: return null;
                }
            }
        }

        public double BackgroundResolution
        {
            get
            {
                if (parentEditor.Imagery != null)
                {
                    switch (BackgroundMode)
                    {
                        case BackgroundMode.Satellite: return parentEditor.Imagery.Resolution;
                        case BackgroundMode.TextureMask: return parentEditor.Imagery.Resolution / parentEditor.Imagery.IdMapMultiplier;
                    }
                }
                return 1;
            }
        }

        public BackgroundMode BackgroundMode
        {
            get { return _backgroundMode; }
            set
            {
                if (_backgroundMode != value)
                {
                    _backgroundMode = value;
                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(nameof(BackgroundImage));
                    NotifyOfPropertyChange(nameof(BackgroundResolution));
                }
            }
        }

        public EditTool EditTool
        {
            get { return _editTool; }
            set
            {
                if (_editTool != value) // Value was sent by toolbar
                {
                    _editTool = value;
                    NotifyOfPropertyChange();

                    if (_editTool == EditTool.AddRoad)
                    {
                        _editMode = GrmMapEditMode.InsertPoint;
                        EditPoints = null;
                    }
                    else
                    {
                        _editMode = GrmMapEditMode.None;
                    }
                    NotifyOfPropertyChange(nameof(GrmMapEditMode));
                }
            }
        }

        public GrmMapEditMode GrmMapEditMode
        {
            get { return _editMode; }
            set
            {
                if (_editMode != value) // Value was sent by map control
                {
                    _editMode = value;
                    NotifyOfPropertyChange();

                    if (_editMode == GrmMapEditMode.None)
                    {
                        _editTool = EditTool.Cursor;
                    }
                    else
                    {
                        _editTool = EditTool.AddRoad;
                    }
                    NotifyOfPropertyChange(nameof(EditTool));
                }
            }
        }

        public List<RoadTypeSelectVM> RoadTypes { get; private set; } = new List<RoadTypeSelectVM>();

        public EditableArma3RoadTypeInfos? SelectectedRoadType
        {
            get { return selectectedRoadType; }
            set
            {
                if (selectectedRoadType != value)
                {
                    selectectedRoadType = value;
                    NotifyOfPropertyChange();
                    foreach (var item in RoadTypes)
                    {
                        item.NotifyOfPropertyChange(nameof(item.IsSelected));
                    }
                }
            }
        }

        protected async override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (parentEditor.ConfigFile != null && !string.IsNullOrEmpty(parentEditor.ConfigFile.Roads))
            {
                if (parentEditor.Roads == null)
                {
                    parentEditor.LoadRoads();
                }
            }

            if (parentEditor.Imagery != null)
            {
                SatMap = parentEditor.Imagery.GetSatMap(arma3Data.ProjectDrive);

                IdMap = parentEditor.Imagery.GetIdMap(arma3Data.ProjectDrive, await parentEditor.GetExportMaterialLibrary());
            }

            _ = Task.Run(DoLoadWorld);

            await base.OnInitializeAsync(cancellationToken);
        }

        internal void InvalidateObjects(bool clearHistory = false)
        {
            // Primary list had a mass change, invalidate our copy
            initialList = null;
            Objects = null;
            NotifyOfPropertyChange(nameof(Objects));

            if (clearHistory)
            {
                // We loaded an old version of the world, clear undo/redo history
                UndoRedoManager.Clear();
                objCache.Clear();
            }

            // Load again
            _ = Task.Run(DoLoadWorld);
        }

        private async Task DoLoadWorld()
        {
            var world = parentEditor.World;
            if (world == null)
            {
                return;
            }

            var models = world.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct(StringComparer.OrdinalIgnoreCase);
            var itemsByPath = await IoC.Get<IAssetsCatalogService>().GetItems(models).ConfigureAwait(false);
            var index = new TerrainSpacialIndex<TerrainObjectVM>(SizeInMeters);
            foreach (var obj in world.Objects)
            {
                if (itemsByPath.TryGetValue(obj.Model, out var modelinfo))
                {
                    if (objCache.TryGetValue(obj, out var vm))
                    {
                        vm.Update(modelinfo);
                    }
                    else
                    {
                        vm = new TerrainObjectVM(this, obj, modelinfo);
                        objCache[obj] = vm;
                    }
                    index.Insert(vm);
                }
            }
            index.AddRange(objCache.Values.Where(o => o.IsRemoved));

            initialList = world.Objects;
            Objects = index;
            NotifyOfPropertyChange(nameof(Objects));
        }

        internal void InvalidateRoads()
        {
            if (EditPoints != null)
            {
                EditPoints = null;
            }

            if (SelectedItems != null)
            {
                SelectedItems = null;
            }

            cache.Clear();

            // Clear Undo/Redo history
            UndoRedoManager.Clear();

            var roads = Roads;
            if (roads != null)
            {
                RoadTypes = roads.RoadTypeInfos.Select(rti => new RoadTypeSelectVM(rti, this)).ToList();
                SelectectedRoadType = roads.RoadTypeInfos.FirstOrDefault();
            }
            else
            {
                RoadTypes = new List<RoadTypeSelectVM>();
            }

            NotifyOfPropertyChange(nameof(Roads));
        }


        void ICommandHandler<SaveFileCommandDefinition>.Update(Command command)
        {
            ((ICommandHandler<SaveFileCommandDefinition>)parentEditor).Update(command);
        }

        Task ICommandHandler<SaveFileCommandDefinition>.Run(Command command)
        {
            return ((ICommandHandler<SaveFileCommandDefinition>)parentEditor).Run(command);
        }

        void ICommandHandler<SaveFileAsCommandDefinition>.Update(Command command)
        {
            ((ICommandHandler<SaveFileAsCommandDefinition>)parentEditor).Update(command);
        }

        Task ICommandHandler<SaveFileAsCommandDefinition>.Run(Command command)
        {
            return ((ICommandHandler<SaveFileAsCommandDefinition>)parentEditor).Run(command);
        }

        public Task TakeAerialImages()
        {
            return parentEditor.TakeAerialImages();
        }

        internal void MakeObjectsDirty()
        {
            parentEditor.IsDirty = true;
        }

        internal void FlushObjectEdits()
        {
            var world = parentEditor.World;
            var objects = Objects;
            if (world == null || initialList == null || objects == null)
            {
                return;
            }
            if (world.Objects != initialList)
            {
                // Fail safe, primary list had a mass change without notifying us, invalidate our copy
                InvalidateObjects();
                return;
            }

            var wasAlive = world.Objects.ToHashSet();

            var resurected = objects.Where(r => !r.IsRemoved && !wasAlive.Contains(r.WrpObject)).Select(r => r.WrpObject).ToList();
            var removed = objects.Where(o => o.IsRemoved).ToList();

            if (removed.Count > 0 || resurected.Count > 0)
            {
                var removedWrp = removed.Select(o => o.WrpObject).ToHashSet();
                world.Objects = world.GetNonDummyObjects().Where(o => !removedWrp.Contains(o)).ToList();
                world.Objects.AddRange(resurected);
                world.Objects.Add(EditableWrpObject.Dummy);
                parentEditor.PostEdit();
            }
        }
    }
}
