using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public abstract class GrmMapDrawLayerBase : FrameworkElement, IGrmMapLayer
    {
        private readonly Dictionary<TerrainPolygon, PathGeometry> polyCache = new Dictionary<TerrainPolygon, PathGeometry>();
        private readonly Dictionary<TerrainPath, PathGeometry> pathCache = new Dictionary<TerrainPath, PathGeometry>();

        protected static void SomePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GrmMapDrawLayerBase)?.InvalidateVisual();
        }

        public GrmMap? ParentMap { get; set; }

        public void OnViewportChanged()
        {
            InvalidateVisual();
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

        protected static PathGeometry DoCreatePath(IEnumerable<TerrainPoint> points)
        {
            var path = new PathGeometry();
            path.Figures.Add(CreateFigure(points, false, true));
            return path;
        }

        protected static Point ConvertToPoint(TerrainPoint point)
        {
            return new Point(point.X, point.Y);
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

        protected override void OnRender(DrawingContext dc)
        {
            var map = ParentMap;
            if (map != null)
            {
                dc.PushTransform(map.Translate);
                dc.PushTransform(map.ScaleTr);
                dc.PushTransform(map.BaseDrawTransform);

                DrawMap(dc, map.RenderEnvelope, map.Scale);

                dc.Pop();
                dc.Pop();
                dc.Pop();

                CacheCleanup();
            }
        }

        protected abstract void DrawMap(DrawingContext dc, Envelope envelope, double scale);
    }
}
