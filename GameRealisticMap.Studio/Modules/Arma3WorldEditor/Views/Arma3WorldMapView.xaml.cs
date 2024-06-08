using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.Views
{
    /// <summary>
    /// Logique d'interaction pour Arma3WorldMapView.xaml
    /// </summary>
    public partial class Arma3WorldMapView : UserControl
    {
        public Arma3WorldMapView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ( e.Key == Key.Enter)
            {
                BindingOperations.GetBindingExpression((TextBox)sender, TextBox.TextProperty).UpdateSource();
            }
        }
    }
}
