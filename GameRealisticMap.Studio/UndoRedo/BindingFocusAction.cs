using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class BindingFocusAction<TValue> : IUndoableAction
    {
        private readonly FrameworkElement element;
        private readonly Binding binding;
        private readonly TValue? oldValue;
        private readonly TValue? newValue;

        public BindingFocusAction(FrameworkElement element, Binding binding, TValue? oldValue, TValue? newValue)
        {
            this.element = element;
            this.binding = new Binding() { Path = binding.Path, Converter = binding.Converter, Source = binding.Source ?? element.DataContext, Mode = BindingMode.TwoWay };
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public string Name => $"Change {binding.Path.Path} to {newValue}";

        public void Execute()
        {
            SetTargetValue(newValue);
        }

        public void Undo()
        {
            element.SetValue(Modules.AssetConfigEditor.Views.Behaviors.ValueWhenGotFocusProperty, oldValue);
            SetTargetValue(oldValue);
        }

        private static readonly DependencyProperty DummyProperty = DependencyProperty.RegisterAttached(
            "Dummy",
            typeof(TValue),
            typeof(BindingFocusAction<TValue>));

        private static readonly FrameworkElement DummyElement = new FrameworkElement();

        private void SetTargetValue(TValue? value)
        {
            BindingOperations.SetBinding(DummyElement, DummyProperty, binding);
            DummyElement.SetValue(DummyProperty, value);
            BindingOperations.ClearBinding(DummyElement, DummyProperty);
        }
    }
}