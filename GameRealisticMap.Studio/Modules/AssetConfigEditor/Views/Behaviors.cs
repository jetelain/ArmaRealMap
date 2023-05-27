using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;
using Xceed.Wpf.Toolkit;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views
{
    public static class Behaviors
    {
        private static readonly DependencyProperty UndoRedoManagerProperty =
            DependencyProperty.RegisterAttached("UndoRedoManager", typeof(IUndoRedoManager), typeof(Behaviors), new PropertyMetadata(UndoRedoManagerChanged));

        private static void UndoRedoManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (grid != null)
            {
                grid.CellEditEnding -= Grid_CellEditEnding;
                if (e.NewValue != null)
                {
                    grid.CellEditEnding += Grid_CellEditEnding;
                }
            }
            var textbox = d as TextBox;
            if (textbox != null)
            {
                textbox.GotFocus += (sender, _) => GotFocus<string>((FrameworkElement)sender!, TextBox.TextProperty);
                textbox.LostFocus += (sender, _) => LostFocus<string>((FrameworkElement)sender!, TextBox.TextProperty);
            }
            var colorPicker = d as ColorPicker;
            if (colorPicker != null)
            {
                colorPicker.GotFocus += (sender, _) => GotFocus<System.Windows.Media.Color?>((FrameworkElement)sender!, ColorPicker.SelectedColorProperty);
                colorPicker.LostFocus += (sender, _) => LostFocus<System.Windows.Media.Color?>((FrameworkElement)sender!, ColorPicker.SelectedColorProperty);
            }
        }

        internal static readonly DependencyProperty ValueWhenGotFocusProperty = DependencyProperty.RegisterAttached(
            "ValueWhenGotFocus",
            typeof(object),
            typeof(Behaviors));

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
                    var undoRedo = (IUndoRedoManager)sender.GetValue(UndoRedoManagerProperty);
                    undoRedo.PushAction(new BindingFocusAction<T>(sender, binding, oldValue, newValue));
                }
            }
        }

        private static void Grid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid != null)
            {
                var undoRedo = (IUndoRedoManager)grid.GetValue(UndoRedoManagerProperty);
                var col = e.Column as DataGridBoundColumn;
                if (col != null)
                {
                    undoRedo.PushAction(new BindingAction<string>(e.Row, (Binding)col.Binding));
                }
            }
        }

        public static void SetUndoRedoManager(UIElement target, IUndoRedoManager value)
        {
            target.SetValue(UndoRedoManagerProperty, value);
        }

        public static void SetEnforceScroll(ScrollViewer viewer, bool value)
        {
            viewer.PreviewMouseWheel -= Viewer_PreviewMouseWheel;
            if (value)
            {
                viewer.PreviewMouseWheel += Viewer_PreviewMouseWheel;
            }
        }

        private static void Viewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }


    }
}
