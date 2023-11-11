using System.Collections.Generic;
using System.Collections.Specialized;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public interface IEditablePointCollection : INotifyCollectionChanged, IReadOnlyCollection<TerrainPoint>
    {
        bool CanInsertAtEnds { get; }

        bool CanInsertBetween { get; }

        void Insert(int index, TerrainPoint terrainPoint);

        void RemoveAt(int index);

        void Add(TerrainPoint terrainPoint);

        void Set(int index, TerrainPoint oldValue, TerrainPoint newValue);

        void PreviewSet(int index, TerrainPoint value);
    }
}