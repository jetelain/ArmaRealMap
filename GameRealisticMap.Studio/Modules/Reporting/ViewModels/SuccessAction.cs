using System;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    public class SuccessAction : ICommandWithLabel
    {
        private readonly Action action;

        public SuccessAction(Action action, string label, string description = "")
        {
            this.action = action;
            Label = label;
            Description = description;
        }

        public string Label { get; }

        public string Description { get; }


        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            action();
        }
    }
}
