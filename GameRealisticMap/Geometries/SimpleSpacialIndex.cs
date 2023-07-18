using System.Collections;
using System.Collections.Concurrent;
using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public class SimpleSpacialIndex<T> : IEnumerable<T>
        where T : class
    {
        private class DataNode
        {
            public readonly Vector2 start;
            public readonly Vector2 end;
            public T? value;
            public bool isRemoved;

            public DataNode(T value, Vector2 start, Vector2 end)
            {
                this.value = value;
                this.start = start;
                this.end = end;
            }
        }
        private class LockArea : IDisposable
        {
            public readonly int x1;
            public readonly int y1;
            public readonly int x2;
            public readonly int y2;
            public readonly SimpleSpacialIndex<T> owner;

            public LockArea(int x1, int x2, int y1, int y2, SimpleSpacialIndex<T> owner)
            {
                this.x1 = x1;
                this.x2 = x2;
                this.y1 = y1;
                this.y2 = y2;
                this.owner = owner;
            }

            public void Dispose()
            {
                lock (owner.locks)
                {
                    owner.locks.Remove(this);
                }
            }
        }

        private readonly List<DataNode>[,] cells;
        private readonly ConcurrentQueue<DataNode> all = new ConcurrentQueue<DataNode>();
        private readonly List<LockArea> locks = new List<LockArea>();

        private readonly Vector2 start;
        private readonly Vector2 cellSize;
        private readonly Vector2 maxCell;
        private readonly int maxCellIndex;
        private int removedCount = 0;

        public SimpleSpacialIndex(Vector2 start, Vector2 size)
            : this(start, size, GetCellCount(size))
        {

        }

        public SimpleSpacialIndex(Vector2 start, Vector2 size, int cellCount)
        {
            this.start = start;
            this.cellSize = size / cellCount;
            cells = new List<DataNode>[cellCount, cellCount];
            for(int x = 0; x < cellCount; ++x)
            {
                for (int y = 0; y < cellCount; ++y)
                {
                    cells[x, y] = new List<DataNode>();
                }
            }
            maxCellIndex = cellCount - 1;
            maxCell = new Vector2(maxCellIndex, maxCellIndex);
        }


        public static int GetCellCount(Vector2 size)
        {
            return (int)Math.Max(2, Math.Ceiling(Math.Max(size.X / 80, size.Y / 80)));
        }

        public IEnumerable<T> Values => all.Where(a => !a.isRemoved).Select(a => a.value);

        public int Count => all.Count - removedCount;

        public bool TryLock(Vector2 start, Vector2 end, out IDisposable? locker)
        {
            var p1 = Vector2.Clamp((start - this.start) / cellSize, Vector2.Zero, maxCell);
            var p2 = Vector2.Clamp((end - this.start) / cellSize, Vector2.Zero, maxCell);
            var x1 = (int)Math.Floor(p1.X);
            var x2 = (int)Math.Ceiling(p2.X);
            var y1 = (int)Math.Floor(p1.Y);
            var y2 = (int)Math.Ceiling(p2.Y);
            lock (locks)
            {
                if (locks.Any(a => a.x1 <= x2 &&
                    a.y1 <= y2 &&
                    a.x2 >= x1 &&
                    a.y2 >= y1))
                {
                    locker = null;
                    return false;
                }
                var lockArea = new LockArea( x1: x1, x2: x2, y1: y1, y2: y2, owner: this );
                locker = lockArea;
                locks.Add(lockArea);
            }
            return true;
        }
        private IEnumerable<List<DataNode>> GetCells(Vector2 start, Vector2 end)
        {
            var p1 = Vector2.Clamp((start - this.start) / cellSize, Vector2.Zero, maxCell);
            var p2 = Vector2.Clamp((end - this.start) / cellSize, Vector2.Zero, maxCell);
            var x1 = (int)Math.Floor(p1.X); 
            var x2 = (int)Math.Ceiling(p2.X);
            var y1 = (int)Math.Floor(p1.Y);
            var y2 = (int)Math.Ceiling(p2.Y);
            for (int x = x1; x <= x2; ++x)
            {
                for (int y = y1; y <= y2; ++y)
                {
                    yield return cells[x, y];
                }
            }
        }
        public void Insert(Vector2 point, T value)
        {
            Insert(point, point, value);
        }

        public void Insert(Vector2 start, Vector2 end, T value)
        {
            var node = new DataNode(value, start, end);
            all.Enqueue(node);
            foreach (var cell in GetCells(node.start, node.end))
            {
                cell.Add(node);
            }
        }

        public List<T> Search(Vector2 start, Vector2 end)
        {
            var result = new HashSet<DataNode>();

            foreach (var cell in GetCells(start, end))
            {
                foreach (var node in cell)
                {
                    if (node.start.X <= end.X &&
                        node.start.Y <= end.Y && 

                        node.end.X >= start.X &&
                        node.end.Y >= start.Y &&

                        !node.isRemoved)
                    {
                        result.Add(node);
                    }
                }
            }
            return result.Select(n => n.value).ToList();
        }

        public void Remove(Vector2 start, Vector2 end, T obj)
        {
            DataNode? node = null;
            foreach (var cell in GetCells(start, end))
            {
                if (node == null)
                {
                    node = cell.FirstOrDefault(n => n.value == obj);
                }
                if (node != null)
                {
                    cell.Remove(node);
                }
            }
            if (node != null)
            {
                node.isRemoved = true;
                node.value = null;
                removedCount++;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        public void Clear()
        {
            all.Clear();
            foreach(var list in cells)
            {
                list.Clear();
            }
            removedCount = 0;
        }
    }
}
