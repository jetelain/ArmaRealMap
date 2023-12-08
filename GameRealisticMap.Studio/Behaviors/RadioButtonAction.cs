using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Gemini.Modules.UndoRedo;
using MahApps.Metro.Controls;

namespace GameRealisticMap.Studio.Behaviors
{
    internal class RadioButtonAction : IUndoableAction
    {
        private readonly RadioButton current;
        private readonly RadioButton previous;

        public RadioButtonAction(RadioButton current, RadioButton previous)
        {
            this.current = current;
            this.previous = previous;
        }

        public string Name => current.FindChild<TextBlock>()?.Text ?? "Change radio button";

        public void Execute()
        {
            current.IsChecked = true;
        }

        public void Undo()
        {
            previous.IsChecked = true;
        }
    }
}