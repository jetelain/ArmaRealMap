using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameRealisticMap.Studio.Modules.CompositionTool.Views
{
    internal class CanvasWithGrid : Canvas
    {
        public CanvasWithGrid() 
        {
            var blackStep = CanvasGrid.Scale * 5;
            for (var i = CanvasGrid.Scale; i < CanvasGrid.Size; i += CanvasGrid.Scale)
            {
                if (i % blackStep != 0)
                {
                    Children.Add(new Line()
                    {
                        X1 = 0,
                        X2 = CanvasGrid.Size,
                        Y1 = i,
                        Y2 = i,
                        Width = CanvasGrid.Size,
                        Height = CanvasGrid.Size,
                        Stroke = new SolidColorBrush(Colors.Gray)
                    });
                    Children.Add(new Line()
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
                    Children.Add(new Line()
                    {
                        X1 = 0,
                        X2 = CanvasGrid.Size,
                        Y1 = i,
                        Y2 = i,
                        Width = CanvasGrid.Size,
                        Height = CanvasGrid.Size,
                        Stroke = new SolidColorBrush(Colors.Black)
                    });
                    Children.Add(new Line()
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
