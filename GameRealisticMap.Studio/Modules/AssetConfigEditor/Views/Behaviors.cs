using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.UndoRedo;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views
{
    public static class Behaviors
    {
        private static readonly DependencyProperty UndoRedoManagerProperty =
            DependencyProperty.RegisterAttached("UndoRedoManager", typeof(IUndoRedoManager), typeof(Behaviors), new PropertyMetadata(UndoRedoManagerChanged));

        private static void UndoRedoManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if ( grid != null )
            { 
                grid.CellEditEnding -= Grid_CellEditEnding;
                if ( e.NewValue != null )
                {
                    grid.CellEditEnding += Grid_CellEditEnding;
                }
            }
            else
            {
                var textbox = d as TextBox;
                if ( textbox != null )
                {
                    textbox.GotFocus += Textbox_GotFocus; ;
                    textbox.LostFocus += Textbox_LostFocus;
                }
            }
        }

        private static readonly DependencyProperty TextWhenGotFocusProperty = DependencyProperty.RegisterAttached(
            "TextWhenGotFocus",
            typeof(string),
            typeof(Behaviors));

        private static void Textbox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.SetValue(TextWhenGotFocusProperty, textBox.Text);
            }
        }

        private static void Textbox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var binding = BindingOperations.GetBinding(textBox, TextBox.TextProperty);
                if (binding != null )
                {
                    var oldText = textBox.GetValue(TextWhenGotFocusProperty) as string;
                    if( oldText != textBox.Text )
                    {
                        var undoRedo = (IUndoRedoManager)textBox.GetValue(UndoRedoManagerProperty);
                        undoRedo.PushAction(new StringBindingAction(textBox.DataContext, binding, oldText));
                    }
                }
            }
        }

        private static void Grid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            var grid = sender as DataGrid;
            if ( grid != null )
            {
                var undoRedo = (IUndoRedoManager)grid.GetValue(UndoRedoManagerProperty);
                var col = e.Column as DataGridBoundColumn;
                if (col != null)
                {
                    undoRedo.PushAction(new StringBindingAction(e.Row.DataContext, (Binding)col.Binding));
                }
            }
        }

        public static void SetUndoRedoManager(DataGrid target, IUndoRedoManager value)
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

        private static readonly DependencyProperty DropTargetProperty = 
            DependencyProperty.RegisterAttached("DropTarget", typeof(object), typeof(Behaviors), new PropertyMetadata(DropTargetChanged));
        public static void SetDropTarget(UIElement target, object value)
        {
            target.SetValue(DropTargetProperty, value);
        }

        public static void DropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (UIElement)d;
            viewer.DragEnter -= Viewer_DragEnter;
            viewer.Drop -= Viewer_Drop;
            var target = viewer.GetValue(DropTargetProperty) as CompositionImporter;
            if (target != null)
            {
                viewer.DragEnter += Viewer_DragEnter;
                viewer.Drop += Viewer_Drop;
                viewer.AllowDrop = true;
            }
            else
            {
                viewer.AllowDrop = false;
            }
        }

        private static void Viewer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var p3d = ((string[])e.Data.GetData(DataFormats.FileDrop))
                    .Where(f => string.Equals(System.IO.Path.GetExtension(f), ".p3d"));

                var target = ((UIElement)sender).GetValue(DropTargetProperty) as CompositionImporter;
                if (target != null)
                {
                    target.FromFiles(p3d);
                }
                e.Handled = true;
            }
        }

        private static void Viewer_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (((string[])e.Data.GetData(DataFormats.FileDrop)).Select(f => System.IO.Path.GetExtension(f)).All(e => string.Equals(e, ".p3d")))
                {
                    e.Effects = DragDropEffects.Link;
                }
                e.Handled = true;
            }
        }
    }
}
