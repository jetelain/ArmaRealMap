using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    /// <summary>
    /// An <see cref="ObservableCollection{T}"/> whose <c>Add</c>, <c>Remove</c>, and <c>Move</c>
    /// operations can be recorded in a Gemini <c>IUndoRedoManager</c>.
    /// Use <see cref="UndoRedoManagerExtensions"/> to bind this collection to an undo/redo manager
    /// so that all mutations are undoable in the Studio editor.
    /// Also supports bulk <c>AddRange</c> / <c>RemoveRange</c> with a single change notification.
    /// </summary>
    public class UndoableObservableCollection<T> : ObservableCollection<T>, IList<T>
    {
        public UndoableObservableCollection()
        {
        }

        public UndoableObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public UndoableObservableCollection(List<T> collection)
            : base(collection)
        {
        }

        public void AddRange(IEnumerable<T> items)
        {
            CheckReentrancy();
            foreach (var item in items)
            {
                Items.Add(item);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void RemoveRange(int index, int count)
        {
            if (Items.Count - index < count)
            {
                throw new ArgumentException();
            }
            CheckReentrancy();
            for(int removeIndex = index + count - 1; removeIndex >= index; removeIndex--)
            {
                Items.RemoveAt(removeIndex);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void ClearUndoable(IUndoRedoManager manager)
        {
            manager.ExecuteAction(new ClearObservableCollection<T>(this));
        }

        public void ClearUndoable(Action<IUndoableAction> execute)
        {
            execute(new ClearObservableCollection<T>(this));
        }
    }
}
