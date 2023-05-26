using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.Views;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;
using MahApps.Metro.Controls;

namespace GameRealisticMap.Studio.Modules.Explorer.Views
{
    /// <summary>
    /// Logique d'interaction pour ExplorerView.xaml
    /// </summary>
    public partial class ExplorerView : UserControl
    {
        public ExplorerView()
        {
            InitializeComponent();
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fw = sender as TreeViewItem;
            if (fw != null && fw.IsSelected)
            {
                if (fw.DataContext is IDocument doc)
                {
                    _ = IoC.Get<IShell>().OpenDocumentAsync(doc);
                    e.Handled = true;
                }
            }
        }
        private void StackPanel_Initialized(object sender, EventArgs e)
        {
            var fw = sender as FrameworkElement;
            if (fw != null && fw.DataContext is IModelImporterTarget)
            {
                var treeViewItem = fw.GetVisualAncestor<TreeViewItem>();
                BindingOperations.SetBinding(treeViewItem, Behaviors.DropTargetProperty, new Binding("CompositionImporter"));
                treeViewItem.DragEnter += TreeViewItem_DragEnter;
            }
        }

        private void TreeViewItem_DragEnter(object sender, DragEventArgs e)
        {
            var ti = sender as TreeViewItem;
            if (ti != null)
            {
                ti.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
