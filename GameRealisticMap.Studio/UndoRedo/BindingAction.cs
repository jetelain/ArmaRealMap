using System.Windows;
using System.Windows.Data;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal class BindingAction<TValue> : IUndoableAction
    {
        private readonly Binding binding;
        private readonly TValue? oldValue;
        private bool hasNewValue;
        private TValue? newValue;

        public BindingAction(FrameworkElement element, Binding binding)
        {
            this.binding = new Binding() { Path = binding.Path, Converter = binding.Converter, Source = binding.Source ?? element.DataContext, Mode = BindingMode.TwoWay };
            oldValue = EvaluteCurrentValue();
        }

        public string Name => $"Change {binding.Path.Path}";

        public void Execute()
        {
            SetTargetValue(newValue);
        }

        public void Undo()
        {
            if (!hasNewValue)
            {
                newValue = EvaluteCurrentValue();
                hasNewValue = true;
            }
            SetTargetValue(oldValue);
        }

        private static readonly DependencyProperty DummyProperty = DependencyProperty.RegisterAttached(
            "Dummy",
            typeof(TValue),
            typeof(BindingAction<TValue>));

        private static readonly FrameworkElement DummyElement = new FrameworkElement();

        private TValue? EvaluteCurrentValue()
        {
            BindingOperations.SetBinding(DummyElement, DummyProperty, binding);
            var result = (TValue?)DummyElement.GetValue(DummyProperty);
            BindingOperations.ClearBinding(DummyElement, DummyProperty);
            return result;
        }

        private void SetTargetValue(TValue? value)
        {
            BindingOperations.SetBinding(DummyElement, DummyProperty, binding);
            DummyElement.SetValue(DummyProperty, value);
            BindingOperations.ClearBinding(DummyElement, DummyProperty);
        }
    }
}