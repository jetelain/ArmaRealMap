using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.Inspector;
using Gemini.Modules.Inspector.Inspectors;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    public class EditRoadEditablePointCollection : PropertyChangedBase, IEditablePointCollection, IInspectableObject
    {
        private readonly UndoableObservableCollection<TerrainPoint> points;
        private readonly EditableArma3Road road;
        private readonly Arma3WorldMapViewModel editor;

        internal EditRoadEditablePointCollection(EditableArma3Road road, Arma3WorldMapViewModel editor)
        {
            points = new UndoableObservableCollection<TerrainPoint>(road.Path.Points);
            points.CollectionChanged += Points_CollectionChanged;
            this.road = road;
            this.editor = editor;

            Inspectors =
                new InspectableObjectBuilder()
                    .WithEditor(this, r => r.RoadTypeInfos, new RoadTypeInfosInspectorViewModel(editor))
                    .WithEditor(this, r => r.Order, new TextBoxEditorViewModel<int>())
                    .ToInspectableObject()
                    .Inspectors;
        }

        internal EditableArma3Road Road => road;

        internal UndoableObservableCollection<TerrainPoint> Points => points;

        internal Arma3WorldMapViewModel Editor => editor;

        private void Points_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            road.IsRemoved = points.Count < 2;
            if (points.Count > 0)
            {
                road.Path = new TerrainPath(points.ToList());
            }
        }

        public void Insert(int index, TerrainPoint terrainPoint)
        {
            points.InsertUndoable(Execute, index, terrainPoint);
        }

        public void RemoveAt(int index)
        {
            points.RemoveAtUndoable(Execute, index);
        }

        public void Add(TerrainPoint terrainPoint)
        {
            points.AddUndoable(Execute, terrainPoint);
        }

        public void Set(int index, TerrainPoint oldValue, TerrainPoint newValue)
        {
            points.SetUndoable(Execute, index, oldValue, newValue);
        }

        public void PreviewSet(int index, TerrainPoint value)
        {
            points[index] = value;
        }

        public IEnumerator<TerrainPoint> GetEnumerator()
        {
            return points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SplitAt(int index)
        {
            editor.UndoRedoManager.ExecuteAction(new SplitRoadAction(this, index));
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add { points.CollectionChanged += value; }
            remove { points.CollectionChanged -= value; }
        }

        public int Count => points.Count;

        public bool CanInsertAtEnds => true;

        public bool CanInsertBetween => true;

        public bool CanSplit => true;

        public void EnsureFocus()
        {
            editor.EditPoints = this;
        }

        private void Execute(IUndoableAction action)
        {
            editor.UndoRedoManager.ExecuteAction(new UndoableFocusWrapper(action, EnsureFocus));
        }

        public void Remove()
        {
            points.ClearUndoable(Execute);
        }

        public EditableArma3RoadTypeInfos RoadTypeInfos
        {
            get { return road.RoadTypeInfos; }
            set
            {
                if (road.RoadTypeInfos != value)
                {
                    EnsureFocus();
                    road.RoadTypeInfos = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public IEnumerable<IInspector> Inspectors { get; }

        public int Order
        {
            get { return road.Order; }
            set
            {
                if (road.Order != value)
                {
                    EnsureFocus();
                    road.Order = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanDeletePoint => true;

        public bool IsObjectSquare => false;
    }
}
