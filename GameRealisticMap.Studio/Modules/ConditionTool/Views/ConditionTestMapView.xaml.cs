using System.Windows;
using System.Windows.Controls;
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ConditionTestMapViewModel;
            if (vm != null)
            {
                await vm.RunViewport(map.GetViewportEnveloppe());
            }
        }
    }
}
