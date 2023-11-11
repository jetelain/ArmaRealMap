using System.Collections.Generic;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal static class CollectionExtensions
    {
        public static void AddUndoable<T>(this ICollection<T> collection, IUndoRedoManager manager, T item)
        {
            manager.ExecuteAction(new AddToCollectionAction<T>(collection, item));
        }

        public static void RemoveUndoable<T>(this ICollection<T> collection, IUndoRedoManager manager, T item)
        {
            manager.ExecuteAction(new RemoveFromCollectionAction<T>(collection, item));
        }

        public static void AddUndoable<T>(this IList<T> collection, IUndoRedoManager manager, T item)
        {
            InsertUndoable(collection, manager, collection.Count, item);
        }

        public static void RemoveUndoable<T>(this IList<T> collection, IUndoRedoManager manager, T item)
        {
            var index = collection.IndexOf(item);
            if (index != -1)
            {
                RemoveAtUndoable(collection, manager, index);
            }
            else
            {
                manager.ExecuteAction(new RemoveFromCollectionAction<T>(collection, item));
            }
        }

        public static void RemoveAtUndoable<T>(this IList<T> collection, IUndoRedoManager manager, int index)
        {
            manager.ExecuteAction(new RemoveAtListAction<T>(collection, index));
        }

        public static void InsertUndoable<T>(this IList<T> collection, IUndoRedoManager manager, int index, T item)
        {
            manager.ExecuteAction(new InsertListAction<T>(collection, index, item));
        }

        public static void SetUndoable<T>(this IList<T> collection, IUndoRedoManager manager, int index, T oldValue, T newValue)
        {
            manager.ExecuteAction(new SetListAction<T>(collection, index, oldValue, newValue));
        }

        public static void SetUndoable<T>(this IList<T> collection, IUndoRedoManager manager, int index, T newValue)
        {
            manager.ExecuteAction(new SetListAction<T>(collection, index, collection[index], newValue));
        }
    }
}
