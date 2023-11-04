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

        public GrmMapDraggableSquare(GrmMapBase map, TerrainPoint terrainPoint)
        {
            this.map = map;
            Width = 16;
            Height = 16;
            TerrainPoint = terrainPoint;
        }

        public SolidColorBrush Fill { get; set; } = new SolidColorBrush(Colors.White);

        public Pen Pen { get; set; } = new Pen(new SolidColorBrush(Colors.Red), 1);

        public TerrainPoint TerrainPoint { get; set; }

        public event EventHandler? Edited;

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(Fill, Pen, new Rect(RenderSize));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            start = e.GetPosition(map);
            initialOffset = VisualTreeHelper.GetOffset(this);
            Cursor = Cursors.SizeAll;
            e.Handled = true;
            CaptureMouse();
            base.OnMouseLeftButtonDown(e);
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
                Edited?.Invoke(this, EventArgs.Empty);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            base.OnMouseLeftButtonUp(e);
            //Edited?.Invoke(this, EventArgs.Empty);
        }
    }
}
