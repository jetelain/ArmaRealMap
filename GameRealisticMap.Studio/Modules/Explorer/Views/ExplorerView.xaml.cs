using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.CompositionTool.Behaviors;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit.Core.Utilities;

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

        private async void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fw = sender as TreeViewItem;
            if (fw != null && fw.IsSelected)
            {
                if (fw.DataContext is IDocument doc)
                {
                    e.Handled = true;
                    await IoC.Get<IShell>().OpenDocumentAsync(doc);
                    if (await GetView(doc) is IScrollableView scrollable)
                    {
                        scrollable.ScrollIntoView(null);
                    }
                }
                else 
                {
                    var parentFw = VisualTreeHelperEx.FindAncestorByType<TreeViewItem>(VisualTreeHelper.GetParent(fw));
                    if (parentFw?.DataContext is IDocument parent)
                    {
                        e.Handled = true;
                        await IoC.Get<IShell>().OpenDocumentAsync(parent);
                        if (await GetView(parent) is IScrollableView scrollable)
                        {
                            scrollable.ScrollIntoView(fw.DataContext);
                        }
                    }
                }
            }
        }
        private static async Task<object?> GetView(object viewModel)
        {
            if (viewModel is IViewAware viewAware)
            {
                return await GetView(viewAware);
            }
            return null;
        }

        private static async Task<object?> GetView(IViewAware viewAware)
        {
            var view = viewAware.GetView();
            if (view == null)
            {
                // View is not yet attached
                var t = new TaskCompletionSource();
                var x = new EventHandler<ViewAttachedEventArgs>((_, _) => t.SetResult());
                viewAware.ViewAttached += x;
                await t.Task;
                viewAware.ViewAttached -= x;
                view = viewAware.GetView();
            }
            return view;
        }

        private void StackPanel_Initialized(object sender, EventArgs e)
        {
            var fw = sender as FrameworkElement;
            if (fw != null && fw.DataContext is IModelImporterTarget)
            {
                var treeViewItem = fw.GetVisualAncestor<TreeViewItem>();
                BindingOperations.SetBinding(treeViewItem, CompositionDragDrop.ImporterProperty, new Binding("CompositionImporter"));
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
