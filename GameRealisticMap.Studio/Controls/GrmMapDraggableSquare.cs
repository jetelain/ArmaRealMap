using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    internal sealed class GrmMapDraggableSquare : FrameworkElement
    {
        private Point start;
        private Vector initialOffset;
        private readonly GrmMapBase map;

        public GrmMapDraggableSquare(GrmMapBase map, TerrainPoint terrainPoint, int index)
        {
            this.map = map;
            Width = 12;
            Height = 12;
            TerrainPoint = terrainPoint;
            Focusable = true;
            Index = index;
        }

        public SolidColorBrush Fill { get; set; } = new SolidColorBrush(Colors.White);

        public SolidColorBrush FillFocus { get; set; } = new SolidColorBrush(Colors.Black);

        public Pen Pen { get; set; } = new Pen(new SolidColorBrush(Colors.Black), 1);

        public TerrainPoint TerrainPoint { get; set; }

        public int Index {  get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Fill, Pen, new Rect(RenderSize));
            if ( IsFocused )
            {
                drawingContext.DrawRectangle(FillFocus, null, new Rect(new Point(2, 2), new Size(RenderSize.Width-4, RenderSize.Height-4)));
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Focus();

            start = e.GetPosition(map);
            initialOffset = VisualTreeHelper.GetOffset(this);

            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            InvalidateVisual();
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var pos = e.GetPosition(map);
                var delta = start - pos;
                var s = DesiredSize;
                var p = initialOffset - delta;
                Arrange(new Rect(new Point(p.X , p.Y), s));
                TerrainPoint = map.ViewportCoordinatesCenter(new Point(p.X, p.Y), RenderSize);
                map.OnPointPositionChanged(this);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (IsFocused)
                {
                    Cursor = Cursors.SizeAll;
                    CaptureMouse();
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            base.OnMouseLeftButtonUp(e);
        }
    }
}
