using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public abstract class GrmMapBase : FrameworkElement
    {
        private readonly Dictionary<TerrainPolygon, PathGeometry> polyCache = new Dictionary<TerrainPolygon, PathGeometry>();
        private readonly Dictionary<TerrainPath, PathGeometry> pathCache = new Dictionary<TerrainPath, PathGeometry>();

        private Point start;
        private Point origin;

        public GrmMapBase()
        {
            ClipToBounds = true;
        }

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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            CaptureMouse();
            start = e.GetPosition(this);
            origin = new Point(Translate.X, Translate.Y);
            Cursor = Cursors.Hand;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                var delta = start - e.GetPosition(this);
                Translate.X = origin.X - delta.X;
                Translate.Y = origin.Y - delta.Y;
                InvalidateVisual();
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var zoom = Math.Max(ScaleTr.ScaleX + (e.Delta > 0 ? 0.25 : -0.25), 0.25);

            var relative = e.GetPosition(this);

            var abosuluteX = (relative.X - Translate.X) / ScaleTr.ScaleX;
            var abosuluteY = (relative.Y - Translate.Y) / ScaleTr.ScaleY;

            ScaleTr.ScaleX = zoom;
            ScaleTr.ScaleY = zoom;

            Translate.X = -(abosuluteX * ScaleTr.ScaleX) + relative.X;
            Translate.Y = -(abosuluteY * ScaleTr.ScaleY) + relative.Y;

            InvalidateVisual();
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

        protected Envelope GetViewportEnveloppe(Rect actualSize, float size)
        {
            var northWest = new TerrainPoint((float)(DeltaX / Scale), (float)(size - (DeltaY / Scale)));
            var southEast = northWest + new Vector2((float)(actualSize.Width / Scale), -(float)(actualSize.Height / Scale));
            return new Envelope(new TerrainPoint(northWest.X, southEast.Y), new TerrainPoint(southEast.X, northWest.Y));
        }

        public ITerrainEnvelope GetViewportEnveloppe() => GetViewportEnveloppe(new Rect(0, 0, ActualWidth, ActualHeight), SizeInMeters);

        protected PathGeometry CreatePolygon(float size, TerrainPolygon poly)
        {
            if (!polyCache.TryGetValue(poly, out var polygon))
            {
                polyCache.Add(poly, polygon = DoCreatePolygon(size, poly));
            }
            return polygon;
        }

        protected static PathGeometry DoCreatePolygon(float size, TerrainPolygon poly, bool isStroked = false)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(poly.Shell, true, isStroked, size));
            foreach (var hole in poly.Holes)
            {
                path.Figures.Add(CreateFigure(hole, true, isStroked, size));
            }
            return path;
        }

        protected PathGeometry CreatePath(float size, TerrainPath poly)
        {
            if (!pathCache.TryGetValue(poly, out var polygon))
            {
                pathCache.Add(poly, polygon = DoCreatePath(size, poly));
            }
            return polygon;
        }

        protected static PathGeometry DoCreatePath(float size, TerrainPath tpath)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(tpath.Points, false, true, size));
            return path;
        }

        protected static Point Convert(TerrainPoint point, float size)
        {
            return new Point(point.X, size - point.Y);
        }

        protected static PathFigure CreateFigure(IEnumerable<TerrainPoint> points, bool isFilled, bool isStroked, float size)
        {
            var figure = new PathFigure
            {
                StartPoint = Convert(points.First(), size),
                IsFilled = isFilled
            };
            figure.Segments.Add(new PolyLineSegment(points.Skip(1).Select(p => Convert(p, size)), isStroked));
            return figure;
        }
    }
}
