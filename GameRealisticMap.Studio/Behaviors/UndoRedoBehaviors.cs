using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;
using Xceed.Wpf.Toolkit;

namespace GameRealisticMap.Studio.Behaviors
{
    public static class UndoRedoBehaviors
    {
        private static readonly DependencyProperty ManagerProperty =
            DependencyProperty.RegisterAttached("Manager", typeof(IUndoRedoManager), typeof(UndoRedoBehaviors), new PropertyMetadata(UndoRedoManagerChanged));

        private static void UndoRedoManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid grid)
            {
                grid.CellEditEnding -= Grid_CellEditEnding;
                if (e.NewValue != null)
                {
                    grid.CellEditEnding += Grid_CellEditEnding;
                }
            }
            else if (d is TextBox textbox)
            {
                textbox.GotFocus += (sender, _) => GotFocus<string>((FrameworkElement)sender!, TextBox.TextProperty);
                textbox.LostFocus += (sender, _) => LostFocus<string>((FrameworkElement)sender!, TextBox.TextProperty);
            }
            else if (d is ColorPicker colorPicker)
            {
                colorPicker.GotFocus += (sender, _) => GotFocus<System.Windows.Media.Color?>((FrameworkElement)sender!, ColorPicker.SelectedColorProperty);
                colorPicker.LostFocus += (sender, _) => LostFocus<System.Windows.Media.Color?>((FrameworkElement)sender!, ColorPicker.SelectedColorProperty);
            }
            else if (d is RangeBase range)
            {
                range.GotFocus += (sender, _) => GotFocus<double>((FrameworkElement)sender!, RangeBase.ValueProperty);
                range.LostFocus += (sender, _) => LostFocus<double>((FrameworkElement)sender!, RangeBase.ValueProperty);
            }
            //else if (d is Selector selector)
            //{
            //    selector.GotFocus += (sender, _) => GotFocus<object>((FrameworkElement)sender!, Selector.SelectedValueProperty);
            //    selector.LostFocus += (sender, _) => LostFocus<object>((FrameworkElement)sender!, Selector.SelectedValueProperty);
            //}
        }

        internal static readonly DependencyProperty ValueWhenGotFocusProperty = DependencyProperty.RegisterAttached(
            "ValueWhenGotFocus",
            typeof(object),
            typeof(UndoRedoBehaviors));

        private static void GotFocus<T>(FrameworkElement sender, DependencyProperty property)
        {
            sender.SetValue(ValueWhenGotFocusProperty, sender.GetValue(property));
        }

        private static void LostFocus<T>(FrameworkElement sender, DependencyProperty property)
        {
            var binding = BindingOperations.GetBinding(sender, property);
            if (binding != null)
            {
                var newValue = (T?)sender.GetValue(property);
                var oldValue = (T?)sender.GetValue(ValueWhenGotFocusProperty);
                if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
                {
                    GetManager(sender).PushAction(new BindingFocusAction<T>(sender, binding, oldValue, newValue));
                }
            }
        }

        private static void Grid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid != null)
            {
                var col = e.Column as DataGridBoundColumn;
                if (col != null)
                {
                    GetManager(grid).PushAction(new BindingAction<string>(e.Row, (Binding)col.Binding));
                }
            }
        }

        public static void SetManager(UIElement target, IUndoRedoManager value)
        {
            target.SetValue(ManagerProperty, value);
        }

        public static IUndoRedoManager GetManager(UIElement target)
        {
            return (IUndoRedoManager)target.GetValue(ManagerProperty);
        }
    }
}
