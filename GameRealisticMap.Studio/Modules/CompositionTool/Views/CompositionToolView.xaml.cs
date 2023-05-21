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

namespace GameRealisticMap.Studio.Modules.CompositionTool.Views
{
    /// <summary>
    /// Logique d'interaction pour CompositionToolView.xaml
    /// </summary>
    public partial class CompositionToolView : UserControl
    {
        public CompositionToolView()
        {
            InitializeComponent();

            var blackStep = CanvasGrid.Scale * 5;

            for (var i = CanvasGrid.Scale; i < CanvasGrid.Size; i += CanvasGrid.Scale)
            {
                if (i % blackStep != 0)
                {
                    BaseLayer.Children.Add(new Line()
                    {
                        X1 = 0,
                        X2 = CanvasGrid.Size,
                        Y1 = i,
                        Y2 = i,
                        Width = CanvasGrid.Size,
                        Height = CanvasGrid.Size,
                        Stroke = new SolidColorBrush(Colors.Gray)
                    });
                    BaseLayer.Children.Add(new Line()
                    {
                        Y1 = 0,
                        Y2 = CanvasGrid.Size,
                        X1 = i,
                        X2 = i,
                        Width = CanvasGrid.Size,
                        Height = CanvasGrid.Size,
                        Stroke = new SolidColorBrush(Colors.Gray)
                    });
                }
            }

            for (var i = blackStep; i < CanvasGrid.Size; i += blackStep)
            {
                if (i != CanvasGrid.HalfSize)
                {
                    BaseLayer.Children.Add(new Line()
                    {
                        X1 = 0,
                        X2 = CanvasGrid.Size,
                        Y1 = i,
                        Y2 = i,
                        Width = CanvasGrid.Size,
                        Height = CanvasGrid.Size,
                        Stroke = new SolidColorBrush(Colors.Black)
                    });
                    BaseLayer.Children.Add(new Line()
                    {
                        Y1 = 0,
                        Y2 = CanvasGrid.Size,
                        X1 = i,
                        X2 = i,
                        Width = CanvasGrid.Size,
                        Height = CanvasGrid.Size,
                        Stroke = new SolidColorBrush(Colors.Black)
                    });
                }
            }
        }
    }
}
