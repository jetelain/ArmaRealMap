using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace ArmaRealMap
{
    public class SimpleSpacialIndex<T> where T : class
    {
        private class DataNode
        {
            public Vector2 start;
            public Vector2 end;
            public T value;
            public bool isRemoved;
        }
        private class LockArea : IDisposable
        {
            public int x1;
            public int y1;
            public int x2;
            public int y2;
            public SimpleSpacialIndex<T> owner;
            public void Dispose()
            {
                lock (owner.locks)
                {
                    owner.locks.Remove(this);
                }
            }
        }

        private readonly List<DataNode>[,] cells;
        private readonly ConcurrentBag<DataNode> all = new ConcurrentBag<DataNode>();
        private readonly List<LockArea> locks = new List<LockArea>();

        private readonly Vector2 start;
        private readonly Vector2 end;
        private readonly Vector2 cellSize;
        private readonly Vector2 maxCell;
        private readonly int maxCellIndex;
        private int removedCount = 0;

        public SimpleSpacialIndex(Vector2 start, Vector2 size, int cellCount = 512)
        {
            this.start = start;
            this.end = start + size;
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

        public IEnumerable<T> Values => all.Where(a => !a.isRemoved).Select(a => a.value);

        public int Count => all.Count - removedCount;

        public bool TryLock(Vector2 start, Vector2 end, out IDisposable locker)
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
                var lockArea = new LockArea() { x1 = x1, x2 = x2, y1 = y1, y2 = y2, owner = this };
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
            var node = new DataNode()
            {
                value = value,
                start = start,
                end = end,
            };
            all.Add(node);
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
                        node.end.Y >= start.Y)
                    {
                        result.Add(node);
                    }
                }
            }
            return result.Select(n => n.value).ToList();
        }

        internal void Remove(Vector2 start, Vector2 end, T obj)
        {
            DataNode node = null;
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
    }
}
