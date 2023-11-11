using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    public class EditRoadEditablePointCollection : IEditablePointCollection
    {
        private readonly ObservableCollection<TerrainPoint> points;
        private readonly EditableArma3Road road;
        private readonly IUndoRedoManager undoRedoManager;

        public EditRoadEditablePointCollection(EditableArma3Road road, IUndoRedoManager undoRedoManager)
        {
            points = new ObservableCollection<TerrainPoint>(road.Path.Points);
            points.CollectionChanged += Points_CollectionChanged;
            this.road = road;
            this.undoRedoManager = undoRedoManager;
        }

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
            points.InsertUndoable(undoRedoManager, index, terrainPoint);
        }

        public void RemoveAt(int index)
        {
            points.RemoveAtUndoable(undoRedoManager, index);
        }

        public void Add(TerrainPoint terrainPoint)
        {
            points.AddUndoable(undoRedoManager, terrainPoint);
        }

        public void Set(int index, TerrainPoint oldValue, TerrainPoint newValue)
        {
            points.SetUndoable(undoRedoManager, index, oldValue, newValue);
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

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add { points.CollectionChanged += value; }
            remove { points.CollectionChanged -= value; }
        }

        public int Count => points.Count;

        public bool CanInsertAtEnds => true;

        public bool CanInsertBetween => true;
    }
}
