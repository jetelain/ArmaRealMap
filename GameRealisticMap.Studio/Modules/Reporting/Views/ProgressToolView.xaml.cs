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

namespace GameRealisticMap.Studio.Modules.Reporting.Views
{
    /// <summary>
    /// Logique d'interaction pour ProgressToolView.xaml
    /// </summary>
    public partial class ProgressToolView : UserControl
    {
        public ProgressToolView()
        {
            InitializeComponent();
        }

        private void ItemInitialized(object sender, EventArgs e)
        {
            var ui = sender as FrameworkElement;
            if (ui != null)
            {
                ui.BringIntoView();
            }
        }
    }
}
