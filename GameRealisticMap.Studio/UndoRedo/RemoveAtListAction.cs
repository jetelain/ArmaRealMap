using System.Collections.Generic;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal sealed class RemoveAtListAction<T> : IUndoableAction
    {
        private readonly IList<T> collection;
        private readonly T item;
        private readonly int index;

        public RemoveAtListAction(IList<T> collection, int index)
        {
            this.collection = collection;
            this.item = collection[index];
            this.index = index;
        }

        public string Name => item?.ToString() ?? $"Remove {typeof(T).Name}";

        public void Execute()
        {
            collection.RemoveAt(index);
        }

        public void Undo()
        {
            collection.Insert(index, item);
        }
    }
}