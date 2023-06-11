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
    }
}
