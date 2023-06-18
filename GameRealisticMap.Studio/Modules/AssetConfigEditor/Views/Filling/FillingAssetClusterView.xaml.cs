using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameRealisticMap.Studio.Modules.Explorer;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views.Filling
{
    /// <summary>
    /// Logique d'interaction pour FillingAssetClusterView.xaml
    /// </summary>
    public partial class FillingAssetClusterView : UserControl, IScrollableView
    {
        public FillingAssetClusterView()
        {
            InitializeComponent();
        }

        public void ScrollIntoView(object? dataContext)
        {
            if (dataContext == null)
            {
                ScrollViewer.ScrollToTop();
            }
            else
            {
                var ui = ItemsControl.ItemContainerGenerator.ContainerFromItem(dataContext) as FrameworkElement;
                if (ui != null)
                {
                    ui.BringIntoView();
                }
            }
        }
    }
}
