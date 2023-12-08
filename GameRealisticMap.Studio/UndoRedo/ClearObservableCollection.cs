using System.Collections.Generic;
using System.Linq;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class ClearObservableCollection<T> : IUndoableAction
    {
        private readonly UndoableObservableCollection<T> collection;
        private readonly List<T> snapshot;

        public ClearObservableCollection(UndoableObservableCollection<T> collection)
        {
            this.collection = collection;
            this.snapshot = collection.ToList();
        }

        public string Name => "Remove";

        public void Execute()
        {
            collection.Clear();
        }

        public void Undo()
        {
            collection.AddRange(snapshot);
        }
    }
}