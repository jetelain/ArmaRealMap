using System.Collections.Generic;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class AddToCollectionAction<T> : IUndoableAction
    {
        private readonly ICollection<T> _collection;
        private readonly T _item;


        public AddToCollectionAction(ICollection<T> collection, T item)
        {
            _collection = collection;
            _item = item;
        }

        public string Name => _item?.ToString() ?? $"Add {typeof(T).Name}";

        public void Execute()
        {
            _collection.Add(_item);
        }

        public void Undo()
        {
            _collection.Remove(_item);
        }
    }
}
