using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class BatchAction : IUndoableAction, IEnumerable<IUndoableAction>
    {
        private readonly List<IUndoableAction> _actions = new List<IUndoableAction>();

        public BatchAction(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public void Add(IUndoableAction action)
        {
            _actions.Add(action);
        }

        public void Execute()
        {
            foreach (var action in _actions) { action.Execute(); }
        }

        public IEnumerator<IUndoableAction> GetEnumerator()
        {
            return _actions.GetEnumerator();
        }

        public void Undo()
        {
            foreach (var action in _actions) { action.Undo(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _actions.GetEnumerator();
        }
    }
}
