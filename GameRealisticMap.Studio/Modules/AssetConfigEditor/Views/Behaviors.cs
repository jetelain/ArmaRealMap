using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views
{
    public static class Behaviors
    {
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
        public static void SetDropTarget(Control target, object value)
        {
            target.SetValue(DropTargetProperty, value);
        }

        public static void DropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (Control)d;
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

                var target = ((Control)sender).GetValue(DropTargetProperty) as CompositionImporter;
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
