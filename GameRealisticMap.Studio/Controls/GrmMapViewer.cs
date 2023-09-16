using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Studio.Shared;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapViewer : FrameworkElement
    {
        private readonly Dictionary<TerrainPolygon, PathGeometry> polyCache = new Dictionary<TerrainPolygon, PathGeometry>();
        private readonly Dictionary<TerrainPath, PathGeometry> pathCache = new Dictionary<TerrainPath, PathGeometry>();

        private Point start;
        private Point origin;

        public GrmMapViewer()
        {
            ClipToBounds = true;
        }

        public Brush Background { get; } = new SolidColorBrush(Colors.White);

        public Brush VoidBackground { get; } = new SolidColorBrush(Colors.Gray);

        public double DeltaX => -Translate.X;

        public double DeltaY => -Translate.Y;

        public double Scale => ScaleTr.ScaleX;

        public TranslateTransform Translate { get; } = new TranslateTransform();

        public ScaleTransform ScaleTr { get; } = new ScaleTransform(1, 1, 0, 0);

        public Dictionary<RoadTypeId,Pen> RoadBrushes { get; } = new Dictionary<RoadTypeId,Pen>();

        public List<TerrainPoint> IsTrue
        {
            get { return (List<TerrainPoint>)GetValue(IsTrueProperty); }
            set { SetValue(IsTrueProperty, value); }
        }

        public static readonly DependencyProperty IsTrueProperty =
            DependencyProperty.Register(nameof(IsTrue), typeof(List<TerrainPoint>), typeof(GrmMapViewer), new PropertyMetadata(new List<TerrainPoint>(), SomePropertyChanged));


        public List<TerrainPoint> IsFalse
        {
            get { return (List<TerrainPoint>)GetValue(FalseListProperty); }
            set { SetValue(FalseListProperty, value); }
        }
        public static readonly DependencyProperty FalseListProperty =
            DependencyProperty.Register(nameof(IsFalse), typeof(List<TerrainPoint>), typeof(GrmMapViewer), new PropertyMetadata(new List<TerrainPoint>(), SomePropertyChanged));



        public PreviewMapData? MapData
        {
            get { return (PreviewMapData?)GetValue(MapDataProperty); }
            set { SetValue(MapDataProperty, value); }
        }

        public static readonly DependencyProperty MapDataProperty =
            DependencyProperty.Register(nameof(MapData), typeof(PreviewMapData), typeof(GrmMapViewer), new PropertyMetadata(null, SomePropertyChanged));



        public float SizeInMeters
        {
            get { return (float)GetValue(SizeInMetersProperty); }
            set { SetValue(SizeInMetersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SizeInMeters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeInMetersProperty =
            DependencyProperty.Register("SizeInMeters", typeof(float), typeof(GrmMapViewer), new PropertyMetadata(2500f, SomePropertyChanged));

        private static void SomePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GrmMapViewer)?.SomePropertyChanged();
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

        protected override void OnRender(DrawingContext dc)
        {
            var actualSize = new Rect(0, 0, ActualWidth, ActualHeight);
            if (actualSize.Width == 0 || actualSize.Height == 0)
            {
                return;
            }

            dc.DrawRectangle(VoidBackground, null, actualSize);

            var size = SizeInMeters;

            var enveloppe = GetViewportEnveloppe(actualSize, size);

            dc.PushTransform(Translate);
            dc.PushTransform(ScaleTr);

            dc.DrawRectangle(Background, null, new Rect(Convert(TerrainPoint.Empty, size), Convert(new TerrainPoint(size, size), size)));
            if (MapData != null)
            {
                DrawPolygons(dc, size, enveloppe, GrmMapStyle.OceanBrush, MapData.Ocean.Polygons);
                DrawPolygons(dc, size, enveloppe, GrmMapStyle.OceanBrush, MapData.ElevationWithLakes.Lakes.Select(l => l.TerrainPolygon));
                DrawPolygons(dc, size, enveloppe, GrmMapStyle.OceanBrush, MapData.Watercourses.Polygons);
                DrawPolygons(dc, size, enveloppe, GrmMapStyle.ScrubsBrush, MapData.Scrub.Polygons);
                DrawPolygons(dc, size, enveloppe, GrmMapStyle.ForestBrush, MapData.Forest.Polygons);
                if (Scale > 0.5)
                {
                    DrawPolygons(dc, size, enveloppe, GrmMapStyle.BuildingBrush, MapData.Buildings.Buildings.Select(b => b.Box.Polygon));
                }
                foreach (var road in MapData.Roads.Roads.OrderByDescending(r => r.RoadType))
                {
                    if (road.EnveloppeIntersects(enveloppe))
                    {
                        if (!RoadBrushes.TryGetValue(road.RoadType, out var pen))
                        {
                            RoadBrushes.Add(road.RoadType, pen = new Pen(GrmMapStyle.RoadBrush, road.RoadTypeInfos.Width));
                        }
                        dc.DrawGeometry(null, pen, CreatePath(size, road.Path));
                    }
                }
                var paths = MapData.Railways.Railways.Select(r => r.Path);

                DrawPaths(dc, size, enveloppe, GrmMapStyle.RailwayPen, paths);

                DrawAdditionals(dc, size, enveloppe, MapData.Additional);
            }
            if (Scale > 0.7)
            {
                foreach (var point in IsFalse)
                {
                    if (point.EnveloppeIntersects(enveloppe))
                    {
                        dc.DrawEllipse(GrmMapStyle.FalseFill, GrmMapStyle.FalsePen, Convert(point, size), 3, 3);
                    }
                }
                foreach (var point in IsTrue)
                {
                    if (point.EnveloppeIntersects(enveloppe))
                    {
                        dc.DrawEllipse(GrmMapStyle.TrueFill, GrmMapStyle.TruePen, Convert(point, size), 3, 3);
                    }
                }
            }
            dc.Pop();
            dc.Pop();
            if (polyCache.Count > 50000)
            {
                polyCache.Clear();
            }
            if (pathCache.Count > 50000)
            {
                pathCache.Clear();
            }
        }

        private void DrawAdditionals(DrawingContext dc, float size, Envelope enveloppe, List<PreviewAdditionalLayer> layers)
        {
            foreach (var additional in layers)
            {
                if (additional.Polygons != null)
                {
                    DrawPolygons(dc, size, enveloppe, GrmMapStyle.GetAdditionalBrush(additional.Name)!, additional.Polygons);
                }
                else if (additional.Paths != null)
                {
                    if (Scale > 0.5)
                    {
                        DrawPaths(dc, size, enveloppe, GrmMapStyle.GetAdditionalPen(additional.Name)!, additional.Paths);
                    }
                }
                else if (additional.Points != null)
                {
                    if (Scale > 0.7)
                    {
                        DrawPoints(dc, size, enveloppe, additional.Points, GrmMapStyle.GetAdditionalPen(additional.Name), GrmMapStyle.GetAdditionalBrush(additional.Name), 2);
                    }
                }
            }
        }

        private static void DrawPoints(DrawingContext dc, float size, Envelope enveloppe, IEnumerable<TerrainPoint> points, Pen? pen, Brush? brush, int radius)
        {
            foreach (var point in points)
            {
                if (point.EnveloppeIntersects(enveloppe))
                {
                    dc.DrawEllipse(brush, pen, Convert(point, size), radius, radius);
                }
            }
        }


        private void DrawPaths(DrawingContext dc, float size, Envelope enveloppe, Pen pen, IEnumerable<TerrainPath> paths)
        {
            foreach (var path in paths)
            {
                if (path.EnveloppeIntersects(enveloppe))
                {
                    dc.DrawGeometry(null, pen, CreatePath(size, path));
                }
            }
        }

        private Envelope GetViewportEnveloppe(Rect actualSize, float size)
        {
            var northWest = new TerrainPoint((float)(DeltaX / Scale), (float)(size - (DeltaY / Scale)));
            var southEast = northWest + new Vector2((float)(actualSize.Width / Scale), -(float)(actualSize.Height / Scale));
            return new Envelope(new TerrainPoint(northWest.X, southEast.Y), new TerrainPoint(southEast.X, northWest.Y));
        }

        public ITerrainEnvelope GetViewportEnveloppe() => GetViewportEnveloppe(new Rect(0, 0, ActualWidth, ActualHeight), SizeInMeters);

        private void DrawPolygons(DrawingContext dc, float size, Envelope enveloppe, Brush brush, IEnumerable<TerrainPolygon> polygons)
        {
            var isLowScale = Scale < 1;
            foreach (var poly in polygons)
            {
                if (isLowScale && poly.EnveloppeArea < 200)
                {
                    continue;
                }
                if (poly.EnveloppeIntersects(enveloppe))
                {
                    dc.DrawGeometry(brush, null, CreatePolygon(size, poly));
                }
            }
        }

        private PathGeometry CreatePolygon(float size, TerrainPolygon poly)
        {
            if (!polyCache.TryGetValue(poly, out var polygon))
            {
                polyCache.Add(poly, polygon = DoCreatePolygon(size, poly));
            }
            return polygon;
        }

        private static PathGeometry DoCreatePolygon(float size, TerrainPolygon poly)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(poly.Shell, true, false, size));
            foreach (var hole in poly.Holes)
            {
                path.Figures.Add(CreateFigure(hole, true, false, size));
            }
            return path;
        }

        private PathGeometry CreatePath(float size, TerrainPath poly)
        {
            if (!pathCache.TryGetValue(poly, out var polygon))
            {
                pathCache.Add(poly, polygon = DoCreatePath(size, poly));
            }
            return polygon;
        }

        private static PathGeometry DoCreatePath(float size, TerrainPath tpath)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(tpath.Points, false, true, size));
            return path;
        }

        private static Point Convert(TerrainPoint point, float size)
        {
            return new Point(point.X, size - point.Y);
        }

        private static PathFigure CreateFigure(IEnumerable<TerrainPoint> points, bool isFilled, bool isStroked, float size)
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
