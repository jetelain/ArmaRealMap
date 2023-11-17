using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public abstract class GrmMapBase : Panel
    {
        private readonly Dictionary<TerrainPolygon, PathGeometry> polyCache = new Dictionary<TerrainPolygon, PathGeometry>();
        private readonly Dictionary<TerrainPath, PathGeometry> pathCache = new Dictionary<TerrainPath, PathGeometry>();

        private Point start;
        private Point origin;

        private readonly double[] ZoomLevelToScale = new double[] { 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 4, 6, 8, 12, 16, 20, 28, 36, 42, 50 };
        private int zoomLevel = 3;

        public GrmMapBase()
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

        protected static void SomePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GrmMapBase)?.SomePropertyChanged();
        }

        private void SomePropertyChanged()
        {
            polyCache.Clear();
            pathCache.Clear();
            InvalidateVisual();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source != this)
            {
                return;
            }
            CaptureMouse();
            start = e.GetPosition(this);
            origin = new Point(Translate.X, Translate.Y);
            Cursor = Cursors.Hand;
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var delta = start - e.GetPosition(this);
                Translate.X = origin.X - delta.X;
                Translate.Y = origin.Y - delta.Y;
                InvalidateVisual();
                InvalidateArrange();
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

            InvalidateVisual();
            InvalidateArrange();
            base.OnMouseWheel(e);
        }

        protected void CacheCleanup()
        {
            if (polyCache.Count > 50000)
            {
                polyCache.Clear();
            }
            if (pathCache.Count > 50000)
            {
                pathCache.Clear();
            }
        }

        public float SizeInMeters
        {
            get { return (float)GetValue(SizeInMetersProperty); }
            set { SetValue(SizeInMetersProperty, value); }
        }

        public static readonly DependencyProperty SizeInMetersProperty =
            DependencyProperty.Register(nameof(SizeInMeters), typeof(float), typeof(GrmMapBase), new PropertyMetadata(2500f, SomePropertyChanged));

        protected Envelope GetViewportEnveloppe(Size actualSize, float size)
        {
            var northWest = new TerrainPoint((float)(DeltaX / Scale), (float)(size - (DeltaY / Scale)));
            var southEast = northWest + new Vector2((float)(actualSize.Width / Scale), -(float)(actualSize.Height / Scale));
            return new Envelope(new TerrainPoint(northWest.X, southEast.Y), new TerrainPoint(southEast.X, northWest.Y));
        }

        public ITerrainEnvelope GetViewportEnveloppe() => GetViewportEnveloppe(RenderSize, SizeInMeters);

        protected PathGeometry CreatePolygon(TerrainPolygon poly)
        {
            if (!polyCache.TryGetValue(poly, out var polygon))
            {
                polyCache.Add(poly, polygon = DoCreatePolygon(poly));
            }
            return polygon;
        }

        protected static PathGeometry DoCreatePolygon(TerrainPolygon poly, bool isStroked = false)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(poly.Shell, true, isStroked));
            foreach (var hole in poly.Holes)
            {
                path.Figures.Add(CreateFigure(hole, true, isStroked));
            }
            return path;
        }

        protected PathGeometry CreatePath(TerrainPath poly)
        {
            if (!pathCache.TryGetValue(poly, out var polygon))
            {
                pathCache.Add(poly, polygon = DoCreatePath(poly));
            }
            return polygon;
        }

        protected static PathGeometry DoCreatePath(TerrainPath tpath)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(tpath.Points, false, true));
            return path;
        }

        protected static Point ConvertToPoint(TerrainPoint point)
        {
            return new Point(point.X, point.Y);
        }

        protected static Point ProjectToPoint(TerrainPoint point, float size)
        {
            return new Point(point.X, size - point.Y);
        }

        protected static PathFigure CreateFigure(IEnumerable<TerrainPoint> points, bool isFilled, bool isStroked)
        {
            var figure = new PathFigure
            {
                StartPoint = ConvertToPoint(points.First()),
                IsFilled = isFilled
            };
            figure.Segments.Add(new PolyLineSegment(points.Skip(1).Select(ConvertToPoint), isStroked));
            return figure;
        }

        protected override sealed void OnRender(DrawingContext dc)
        {
            var renderSize = RenderSize;
            if (renderSize.Width == 0 || renderSize.Height == 0)
            {
                return;
            }

            dc.DrawRectangle(Background, null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));

            var size = SizeInMeters;

            var enveloppe = GetViewportEnveloppe(renderSize, size);

            dc.PushTransform(Translate);
            dc.PushTransform(ScaleTr);

            dc.DrawRectangle(MapBackground, null, new Rect(ConvertToPoint(TerrainPoint.Empty), ConvertToPoint(new TerrainPoint(size, size))));

            DrawMap(dc, size, enveloppe);

            dc.Pop();
            dc.Pop();

            CacheCleanup();
        }

        protected abstract void DrawMap(DrawingContext dc, float size, Envelope enveloppe);
        
        public TerrainPoint ViewportCoordinates(Point viewPortPoint)
        {
            var northWest = new TerrainPoint((float)(DeltaX / Scale), (float)(SizeInMeters - (DeltaY / Scale))); // Point(0,0) of view port
            return northWest + new Vector2((float)(viewPortPoint.X / Scale), -(float)(viewPortPoint.Y / Scale));
        }

        public TerrainPoint ViewportCoordinatesCenter(Point point, Size size)
        {
            return ViewportCoordinates(point + new System.Windows.Vector(size.Width / 2, size.Height / 2));
        }
    }
}
