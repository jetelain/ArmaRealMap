using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caliburn.Micro;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ui = sender as FrameworkElement;
            AddMenu.Placement = PlacementMode.Relative;
            AddMenu.PlacementTarget = ui;
            AddMenu.HorizontalOffset = 0;
            AddMenu.VerticalOffset = ui.ActualHeight;
            AddMenu.IsOpen = true;
        }
    }
}
