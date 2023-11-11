using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Views
{
    /// <summary>
    /// Logique d'interaction pour AssetBrowserView.xaml
    /// </summary>
    public partial class AssetBrowserView : UserControl
    {
        public AssetBrowserView()
        {
            InitializeComponent();
        }

        private void StackPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(Grid,
                                     new DataObject ("GRM.A3.Path", Grid.SelectedItems.OfType<AssetViewModel>().Select(i => i.Path).ToArray()),
                                     DragDropEffects.Link);
            }
        }

        private void GridView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left
                && Keyboard.Modifiers == ModifierKeys.None
                && e.OriginalSource is Visual source 
                && sender is ListView list)
            {
                var container = list.ContainerFromElement(source);
                if (container is ListViewItem item)
                {
                    if (item.IsSelected)
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        private void GridView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left 
                && Keyboard.Modifiers == ModifierKeys.None 
                && e.OriginalSource is Visual source 
                && sender is ListView list)
            {
                var container = list.ContainerFromElement(source);
                if (container is ListViewItem item)
                {
                    if (item.IsSelected)
                    {
                        var index = list.ItemContainerGenerator.IndexFromContainer(container);
                        if (index >= 0)
                        {
                            list.SelectedIndex = index;
                        }
                    }
                }
            }
        }

        private void MenuItem_Remove(object sender, RoutedEventArgs e)
        {
            (DataContext as AssetBrowserViewModel)?.RemoveAssetsAsync(Grid.SelectedItems.OfType<AssetViewModel>());
        }

        private void MenuItem_ChangeType(object sender, RoutedEventArgs e)
        {
            var category = ((FrameworkElement)e.OriginalSource).DataContext as CategoryOption;
            if (category != null)
            {
                (DataContext as AssetBrowserViewModel)?.ChangeAssetsCategoryAsync(Grid.SelectedItems.OfType<AssetViewModel>(), category.Category);
            }
        }

        private void MenuItem_Preview3D(object sender, RoutedEventArgs e)
        {
            var asset = Grid.SelectedItems.OfType<AssetViewModel>().FirstOrDefault();
            if (asset != null)
            {
                asset.ShowPreview3D();
            }
        }
    }
}
