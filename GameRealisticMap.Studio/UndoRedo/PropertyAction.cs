using System;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class PropertyAction<TValue> : IUndoableAction
    {
        private readonly TValue newValue;
        private readonly TValue oldValue;
        private readonly string name;
        private readonly Action<TValue> action;

        public PropertyAction(TValue newValue, TValue oldValue, string name, Action<TValue> action) 
        { 
            this.newValue = newValue;
            this.oldValue = oldValue;
            this.action = action;
            this.name = name;
        }

        public string Name => $"Change {name} to {newValue}";

        public void Execute()
        {
            action(newValue);
        }

        public void Undo()
        {
            action(oldValue);
        }
    }
}
