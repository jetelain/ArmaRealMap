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
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.ConditionTool.Views
{
    /// <summary>
    /// Logique d'interaction pour ConditionTestMapView.xaml
    /// </summary>
    public partial class ConditionTestMapView : UserControl
    {
        public ConditionTestMapView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ConditionTestMapViewModel;
            if (vm != null)
            {
                vm.RunViewport(map.GetViewportEnveloppe());
            }
        }
    }
}
