using System.Collections.Generic;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal sealed class InsertListAction<T> : IUndoableAction
    {
        private readonly IList<T> collection;
        private readonly int index;
        private readonly T item;

        public InsertListAction(IList<T> collection, int index, T item)
        {
            this.collection = collection;
            this.index = index;
            this.item = item;
        }

        public string Name => item?.ToString() ?? $"Add {typeof(T).Name}";

        public void Execute()
        {
            collection.Insert(index, item);
        }

        public void Undo()
        {
            collection.RemoveAt(index);
        }
    }
}