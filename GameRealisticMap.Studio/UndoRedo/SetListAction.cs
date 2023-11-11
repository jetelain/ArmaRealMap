using System.Collections.Generic;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal sealed class SetListAction<T> : IUndoableAction
    {
        private readonly IList<T> collection;
        private readonly int index;
        private readonly T oldValue;
        private readonly T newValue;
        private readonly string? name;

        public SetListAction(IList<T> collection, int index, T oldValue, T newValue, string? name = null)
        {
            this.collection = collection;
            this.index = index;
            this.oldValue = oldValue;
            this.newValue = newValue;
            this.name = name;
        }

        public string Name => name ?? newValue?.ToString() ?? $"Change {typeof(T).Name}";

        public void Execute()
        {
            collection[index] = newValue;
        }

        public void Undo()
        {
            collection[index] = oldValue;
        }
    }
}