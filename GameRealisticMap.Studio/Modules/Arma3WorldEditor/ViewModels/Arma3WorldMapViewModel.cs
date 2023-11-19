using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using Gemini.Framework;
using Gemini.Framework.Commands;
using Gemini.Modules.Inspector;
using Gemini.Modules.Shell.Commands;
using HugeImages;
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

        public double BackgroundResolution { get; }

        private BackgroundMode _backgroundMode;
        private EditTool _editTool;
        private GrmMapEditMode _editMode;
        private EditableArma3RoadTypeInfos? selectectedRoadType;
        private IEnumerable<IEditablePointCollection>? selectedItems;

        public Arma3WorldMapViewModel(Arma3WorldEditorViewModel parent, IArma3DataModule arma3Data)
        {
            this.parentEditor = parent;
            this.arma3Data = arma3Data;
            this.inspectorTool = IoC.Get<IInspectorTool>();
            BackgroundResolution = parentEditor.Imagery?.Resolution ?? 1;
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

                    NotifyOfPropertyChange(nameof(CanMerge));
                }
            }
        }

        public TerrainSpacialIndex<TerrainObjectVM>? Objects { get; set; }

        public ICommand SelectItemCommand => new RelayCommand(SelectItem);

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
            var edit = new EditRoadEditablePointCollection(road, this);
            edit.CollectionChanged += MakeRoadsDirty;
            return edit;
        }


        public void SelectItem(object? item)
        {
            SelectedItems = null;

            if (item is EditableArma3Road road)
            {
                EditPoints = CreateEdit(road);
            }
            else
            {
                EditPoints = null;
            }
        }

        public void AddToSelection(object? item)
        {
            if (item is EditableArma3Road road)
            {
                if (selectedItems != null)
                {
                    SelectedItems = selectedItems.Concat(new[] { CreateEdit(road) }).ToList();
                }
                else if (editPoints != null)
                {
                    SelectedItems = new[] { editPoints, CreateEdit(road) }.ToList();
                }
                else
                {
                    EditPoints = CreateEdit(road);
                }
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

                var assets = await parentEditor.GetAssetsFromHistory();
                if (assets != null)
                {
                    IdMap = parentEditor.Imagery.GetIdMap(arma3Data.ProjectDrive, assets.Materials);
                }
            }

            _ = Task.Run(DoLoadWorld);

            await base.OnInitializeAsync(cancellationToken);
        }

        private async Task DoLoadWorld()
        {
            var world = parentEditor.World;
            if (world == null)
            {
                return;
            }

            var models = world.Objects.Select(o => o.Model).Where(m => !string.IsNullOrEmpty(m)).Distinct();

            var itemsByPath = await IoC.Get<IAssetsCatalogService>().GetItems(models).ConfigureAwait(false);

            var index = new TerrainSpacialIndex<TerrainObjectVM>(SizeInMeters);
            foreach (var obj in world.Objects)
            {
                if (itemsByPath.TryGetValue(obj.Model, out var modelinfo))
                {
                    index.Insert(new TerrainObjectVM(obj, modelinfo));
                }
            }
            Objects = index;
            NotifyOfPropertyChange(nameof(Objects));
        }

        internal void InvalidateRoads()
        {
            NotifyOfPropertyChange(nameof(Roads));

            if (EditPoints != null)
            {
                EditPoints = null;
                NotifyOfPropertyChange(nameof(EditPoints));
            }

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
    }
}
