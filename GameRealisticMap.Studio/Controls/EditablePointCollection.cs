using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public class EditablePointCollection : INotifyCollectionChanged
    {
        private readonly ObservableCollection<TerrainPoint> points;
        private readonly EditableArma3Road road;

        public EditablePointCollection(EditableArma3Road road)
        {
            this.points = new ObservableCollection<TerrainPoint>(road.Path.Points);
            points.CollectionChanged += Points_CollectionChanged;
            this.road = road;
        }

        public IReadOnlyList<TerrainPoint> Points => points;

        private void Points_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            road.IsRemoved = points.Count < 2;
            if (points.Count > 0)
            {
                road.Path = new TerrainPath(points.ToList());
            }
        }

        internal void Insert(int index, TerrainPoint terrainPoint)
        {
            points.Insert(index, terrainPoint);
        }

        internal void RemoveAt(int index)
        {
            points.RemoveAt(index);
        }

        internal void Add(TerrainPoint terrainPoint)
        {
            points.Add(terrainPoint);
        }

        internal void Set(int index, TerrainPoint oldValue, TerrainPoint newValue)
        {
            points[index] = newValue;
        }

        internal void PreviewSet(int index, TerrainPoint value)
        {
            points[index] = value;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged
        {
            add { points.CollectionChanged += value; }
            remove { points.CollectionChanged -= value; }
        }

        public int Count => points.Count;

    }
}
