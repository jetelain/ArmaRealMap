using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMap : Panel
    {
        private Point start;
        private Point origin;

        private static readonly double[] ZoomLevelToScale = new double[] { 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6, 8, 12, 16, 20, 28, 36, 42, 50, 75, 100 };
        private int zoomLevel = 3;

        public GrmMap()
        {
            ClipToBounds = true;
            Background = new SolidColorBrush(Colors.Gray);
        }

        public Brush MapBackground { get; } = new SolidColorBrush(Colors.White);

        public double DeltaX => -Translate.X;

        public double DeltaY => -Translate.Y;

        public double Scale => ScaleTr.ScaleX;

        public TranslateTransform Translate { get; } = new TranslateTransform();

        public ScaleTransform ScaleTr { get; } = new ScaleTransform(1, 1, 0, 0);

        public Envelope RenderEnvelope { get; private set; } = new Envelope(new TerrainPoint(0, 0), new TerrainPoint(0, 0));

        public MatrixTransform BaseDrawTransform { get; } = new MatrixTransform(1, 0, 0, -1, 0, 2500);

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            start = e.GetPosition(this);
            origin = new Point(Translate.X, Translate.Y);
            Cursor = Cursors.Hand;
            CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var delta = start - e.GetPosition(this);
                Translate.X = origin.X - delta.X;
                Translate.Y = origin.Y - delta.Y;
                ViewportChanged();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if ( e.Delta > 0 )
            {
                if (zoomLevel < ZoomLevelToScale.Length - 1)
                {
                    zoomLevel++;
                }
            }
            else if (zoomLevel > 0)
            {
                zoomLevel--;
            }

            var zoom = ZoomLevelToScale[zoomLevel];

            var relative = e.GetPosition(this);

            var abosuluteX = (relative.X - Translate.X) / ScaleTr.ScaleX;
            var abosuluteY = (relative.Y - Translate.Y) / ScaleTr.ScaleY;

            ScaleTr.ScaleX = zoom;
            ScaleTr.ScaleY = zoom;

            Translate.X = -(abosuluteX * ScaleTr.ScaleX) + relative.X;
            Translate.Y = -(abosuluteY * ScaleTr.ScaleY) + relative.Y;

            ViewportChanged();

            base.OnMouseWheel(e);
        }

        public float SizeInMeters
        {
            get { return (float)GetValue(SizeInMetersProperty); }
            set { SetValue(SizeInMetersProperty, value); }
        }

        private double SizeInMetersInternal => BaseDrawTransform.Matrix.OffsetY;

        public static readonly DependencyProperty SizeInMetersProperty =
            DependencyProperty.Register(nameof(SizeInMeters), typeof(float), typeof(GrmMap), new PropertyMetadata(2500f, SizeInMetersChanged));

        private static void SizeInMetersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GrmMap)?.SetSizeInMetersInternal((float)e.NewValue);
        }

        private void SetSizeInMetersInternal(float newValue)
        { 
            BaseDrawTransform.Matrix = new Matrix(1, 0, 0, -1, 0, newValue);
            ViewportChanged();
        }

        private Envelope GetViewportEnveloppe(Size actualSize, double size)
        {
            var northWest = new TerrainPoint((float)(DeltaX / Scale), (float)(size - (DeltaY / Scale)));
            var southEast = northWest + new Vector2((float)(actualSize.Width / Scale), -(float)(actualSize.Height / Scale));
            return new Envelope(new TerrainPoint(northWest.X, southEast.Y), new TerrainPoint(southEast.X, northWest.Y));
        }

        public ITerrainEnvelope GetViewportEnveloppe() => GetViewportEnveloppe(RenderSize, SizeInMetersInternal);

        public static Point ProjectToPoint(TerrainPoint point, float size)
        {
            return new Point(point.X, size - point.Y);
        }

        protected override void OnRender(DrawingContext dc)
        {
            var renderSize = RenderSize;
            if (renderSize.Width == 0 || renderSize.Height == 0)
            {
                return;
            }

            dc.DrawRectangle(Background, null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));

            var size = SizeInMetersInternal;

            dc.PushTransform(Translate);
            dc.PushTransform(ScaleTr);

            dc.DrawRectangle(MapBackground, null, new Rect(new Point(0, 0), new Point(size, size)));

            dc.Pop();
            dc.Pop();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            ViewportChanged();
            base.OnRenderSizeChanged(sizeInfo);
        }

        public void ViewportChanged()
        {
            InvalidateVisual();
            RenderEnvelope = GetViewportEnveloppe(RenderSize, BaseDrawTransform.Matrix.OffsetY);
            foreach (var child in InternalChildren.OfType<IGrmMapLayer>())
            {
                child.OnViewportChanged();
            }
        }

        public TerrainPoint ViewportCoordinates(Point viewPortPoint)
        {
            var northWest = new TerrainPoint((float)(DeltaX / Scale), (float)(BaseDrawTransform.Matrix.OffsetY - (DeltaY / Scale))); // Point(0,0) of view port
            return northWest + new Vector2((float)(viewPortPoint.X / Scale), -(float)(viewPortPoint.Y / Scale));
        }

        public TerrainPoint ViewportCoordinatesCenter(Point point, Size size)
        {
            return ViewportCoordinates(point + new System.Windows.Vector(size.Width / 2, size.Height / 2));
        }

        public Point ProjectViewport(TerrainPoint point)
        {
            return Translate.Transform(ScaleTr.Transform(BaseDrawTransform.Transform(new Point(point.X, point.Y))));
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is IGrmMapLayer layerAdded)
            {
                layerAdded.ParentMap = this;
            }
            if (visualRemoved is IGrmMapLayer layerRemoved)
            {
                layerRemoved.ParentMap = null;
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var rect = new Rect(new Point(), RenderSize);
            foreach (UIElement child in InternalChildren)
            {
                child.Arrange(rect);
            }
            return arrangeSize;
        }
    }
}
