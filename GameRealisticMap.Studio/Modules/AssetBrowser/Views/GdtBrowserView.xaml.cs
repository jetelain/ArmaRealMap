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
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Views
{
    /// <summary>
    /// Logique d'interaction pour GdtBrowserView.xaml
    /// </summary>
    public partial class GdtBrowserView : UserControl
    {
        public GdtBrowserView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            ((sender as Hyperlink)?.DataContext as GdtDetailViewModel)?.OpenMaterial();
        }
    }
}
