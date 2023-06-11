using System.Linq;
using System.Windows;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.CompositionTool.Behaviors
{
    public static class CompositionDragDrop
    {
        public static readonly DependencyProperty ImporterProperty =
            DependencyProperty.RegisterAttached("Importer", typeof(CompositionImporter), typeof(CompositionDragDrop), new PropertyMetadata(ImporterChanged));
        
        internal static void SetImporter(UIElement target, CompositionImporter value)
        {
            target.SetValue(ImporterProperty, value);
        }

        private static void ImporterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (UIElement)d;
            viewer.DragEnter -= Viewer_DragEnter;
            viewer.Drop -= Viewer_Drop;
            var target = viewer.GetValue(ImporterProperty) as CompositionImporter;
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

                var target = ((UIElement)sender).GetValue(ImporterProperty) as CompositionImporter;
                if (target != null)
                {
                    target.FromFiles(p3d);
                }
                e.Handled = true;
            }
            if (e.Data.GetDataPresent("GRM.A3.Path"))
            {
                e.Handled = true;
                var target = ((UIElement)sender).GetValue(ImporterProperty) as CompositionImporter;
                if (target != null)
                {
                    target.FromPaths((string[])e.Data.GetData("GRM.A3.Path"));
                }
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
            }
            if (e.Data.GetDataPresent("GRM.A3.Path"))
            {
                e.Effects = DragDropEffects.Link;
            }
        }
    }
}
