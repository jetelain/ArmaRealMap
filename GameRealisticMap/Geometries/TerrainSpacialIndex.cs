using System.Numerics;
using GameRealisticMap;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Geometries
{
    public class TerrainSpacialIndex<T> : SimpleSpacialIndex<T> where T : class, ITerrainEnvelope
    {
        public TerrainSpacialIndex(ITerrainArea map)
            : this(map.SizeInMeters)
        {
        }

        public TerrainSpacialIndex(float sizeInMeters)
            : base(Vector2.Zero, new Vector2(sizeInMeters, sizeInMeters))
        {
        }

        public void Insert(T obj)
        {
            Insert(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (var obj in enumerable)
            {
                Insert(obj);
            }
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

        public IEnumerable<T> Where(ITerrainEnvelope area, Func<T, bool> selector)
        {
            return Search(area).Where(selector);
        }

        public IEnumerable<T> RemoveAll(ITerrainEnvelope area, Func<T, bool> selector)
        {
            var toremove = Search(area).Where(selector).ToList();
            foreach(var item in toremove)
            {
                Remove(item);
            }
            return toremove;
        }
    }
}
