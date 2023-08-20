using System.Windows;
using System.Windows.Controls;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Controls
{
    /// <summary>
    /// Logique d'interaction pour PreviewControl.xaml
    /// </summary>
    public partial class PreviewControl : UserControl
    {
        public PreviewControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PreviewBoxWidthPixelsProperty =
            DependencyProperty.Register(nameof(PreviewBoxWidthPixels), typeof(double), typeof(PreviewControl), new PropertyMetadata(PreviewGrid.Size));

        public double PreviewBoxWidthPixels
        {
            get { return (double)GetValue(PreviewBoxWidthPixelsProperty); }
            set { SetValue(PreviewBoxWidthPixelsProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(PreviewControl), new PropertyMetadata(null));

        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
    }
}
