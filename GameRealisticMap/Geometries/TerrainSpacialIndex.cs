using System.Numerics;
using GameRealisticMap;
using GameRealisticMap.Geometries;

namespace ArmaRealMap
{
    public class TerrainSpacialIndex<T> : SimpleSpacialIndex<T> where T : class, ITerrainEnvelope
    {
        public TerrainSpacialIndex(ITerrainArea map)
            : base(Vector2.Zero, new Vector2(map.SizeInMeters, map.SizeInMeters))
        {
        }

        public void Insert(T obj)
        {
            Insert(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
        }

        public void Remove(T obj)
        {
            Remove(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
        }

        public List<T> Search(TerrainPoint start, TerrainPoint end)
        {
            return Search(start.Vector, end.Vector);
        }

        public List<T> Search(ITerrainEnvelope area)
        {
            return Search(area.MinPoint.Vector, area.MaxPoint.Vector);
        }

        public bool TryLock(ITerrainEnvelope area, out IDisposable? locker)
        {
            return TryLock(area.MinPoint.Vector, area.MaxPoint.Vector, out locker);
        }

        public bool TryLock(ITerrainEnvelope area, Vector2 offset, out IDisposable? locker)
        {
            return TryLock(area.MinPoint.Vector - offset, area.MaxPoint.Vector + offset, out locker);
        }
    }
}
