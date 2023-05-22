using System.Windows;
using System.Windows.Data;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class StringBindingAction : IUndoableAction
    {
        private readonly Binding binding;
        private readonly string? initialValue;
        private string? changedValue;

        public StringBindingAction(object dataContext, Binding binding)
        {
            this.binding = new Binding() { Path = binding.Path, Converter = binding.Converter, Source = binding.Source ?? dataContext, Mode = BindingMode.TwoWay };
            initialValue = EvaluateAsString();
        }

        public StringBindingAction(object dataContext, Binding binding, string? initialValue)
        {
            this.binding = new Binding() { Path = binding.Path, Converter = binding.Converter, Source = binding.Source ?? dataContext, Mode = BindingMode.TwoWay };
            this.initialValue = initialValue;
        }

        public string Name => $"Change {binding.Path.Path} of {binding.Source}";

        public void Execute()
        {
            SetSourceFromString(changedValue);
        }

        public void Undo()
        {
            changedValue = EvaluateAsString();
            SetSourceFromString(initialValue);
        }

        private static readonly DependencyProperty DummyProperty = DependencyProperty.RegisterAttached(
            "Dummy",
            typeof(string),
            typeof(StringBindingAction));

        private static readonly FrameworkElement DummyElement = new FrameworkElement();

        private string? EvaluateAsString()
        {
            BindingOperations.SetBinding(DummyElement, DummyProperty, binding);
            var result = DummyElement.GetValue(DummyProperty) as string;
            BindingOperations.ClearBinding(DummyElement, DummyProperty);
            return result;
        }

        private void SetSourceFromString(string? value)
        {
            BindingOperations.SetBinding(DummyElement, DummyProperty, binding);
            DummyElement.SetValue(DummyProperty, value);
            BindingOperations.ClearBinding(DummyElement, DummyProperty);
        }
    }
}