using System;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class UndoableFocusWrapper : IUndoableAction
    {
        private readonly IUndoableAction action;
        private readonly Action focus;

        public UndoableFocusWrapper(IUndoableAction action, Action focus)
        {
            this.action = action;
            this.focus = focus;
        }

        public string Name => action.Name;

        public void Execute()
        {
            focus();
            action.Execute();
        }

        public void Undo()
        {
            focus();
            action.Undo();
        }
    }
}