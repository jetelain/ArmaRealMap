using System.Windows.Controls;
using GameRealisticMap.Studio.Modules.MapConfigEditor.ViewModels;

namespace GameRealisticMap.Studio.Modules.MapConfigEditor.Views
{
    /// <summary>
    /// Logique d'interaction pour MapPreviewView.xaml
    /// </summary>
    public partial class MapPreviewView : UserControl
    {
        public MapPreviewView()
        {
            InitializeComponent();
        }

        private void DispatchViewOSM(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as MapPreviewViewModel)?.ViewOSM(Map.GetViewportEnveloppe());
        }

        private void DispatchEditOSM(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as MapPreviewViewModel)?.EditOSM(Map.GetViewportEnveloppe());
        }
    }
}
