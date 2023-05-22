using System.Collections.Generic;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class RemoveFromCollectionAction<T> : IUndoableAction
    {
        private readonly ICollection<T> _collection;
        private readonly T _item;


        public RemoveFromCollectionAction(ICollection<T> collection, T item)
        {
            _collection = collection;
            _item = item;
        }

        public string Name =>  $"Remove {_item?.ToString() ?? typeof(T).Name}";

        public void Execute()
        {
            _collection.Remove(_item);
        }

        public void Undo()
        {
            _collection.Add(_item);
        }
    }
}
