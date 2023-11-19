using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Studio.Shared;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapPreviewLayer : GrmMapDrawLayerBase
    {
        public Dictionary<RoadTypeId,Pen> RoadBrushes { get; } = new Dictionary<RoadTypeId,Pen>();

        public List<ITerrainEnvelope> IsTrue
        {
            get { return (List<ITerrainEnvelope>)GetValue(IsTrueProperty); }
            set { SetValue(IsTrueProperty, value); }
        }

        public static readonly DependencyProperty IsTrueProperty =
            DependencyProperty.Register(nameof(IsTrue), typeof(List<ITerrainEnvelope>), typeof(GrmMapPreviewLayer), new PropertyMetadata(new List<ITerrainEnvelope>(), SomePropertyChanged));

        public List<ITerrainEnvelope> IsFalse
        {
            get { return (List<ITerrainEnvelope>)GetValue(FalseListProperty); }
            set { SetValue(FalseListProperty, value); }
        }
        public static readonly DependencyProperty FalseListProperty =
            DependencyProperty.Register(nameof(IsFalse), typeof(List<ITerrainEnvelope>), typeof(GrmMapPreviewLayer), new PropertyMetadata(new List<ITerrainEnvelope>(), SomePropertyChanged));

        public PreviewMapData? MapData
        {
            get { return (PreviewMapData?)GetValue(MapDataProperty); }
            set { SetValue(MapDataProperty, value); }
        }

        public static readonly DependencyProperty MapDataProperty =
            DependencyProperty.Register(nameof(MapData), typeof(PreviewMapData), typeof(GrmMapPreviewLayer), new PropertyMetadata(null, SomePropertyChanged));

        protected override void DrawMap(DrawingContext dc, Envelope enveloppe, double scale)
        {
            if (MapData != null)
            {
                DrawPolygons(dc, enveloppe, GrmMapStyle.OceanBrush, MapData.Ocean.Polygons, scale);
                DrawPolygons(dc, enveloppe, GrmMapStyle.OceanBrush, MapData.ElevationWithLakes.Lakes.Select(l => l.TerrainPolygon), scale);
                DrawPolygons(dc, enveloppe, GrmMapStyle.OceanBrush, MapData.Watercourses.Polygons, scale);
                DrawPolygons(dc, enveloppe, GrmMapStyle.ScrubsBrush, MapData.Scrub.Polygons, scale);
                DrawPolygons(dc, enveloppe, GrmMapStyle.ForestBrush, MapData.Forest.Polygons, scale);
                if (scale > 0.5)
                {
                    DrawPolygons(dc, enveloppe, GrmMapStyle.BuildingBrush, MapData.Buildings.Buildings.Select(b => b.Box.Polygon), scale);
                }
                foreach (var road in MapData.Roads.Roads.OrderByDescending(r => r.RoadType))
                {
                    if (road.EnveloppeIntersects(enveloppe))
                    {
                        if (!RoadBrushes.TryGetValue(road.RoadType, out var pen))
                        {
                            RoadBrushes.Add(road.RoadType, pen = new Pen(GrmMapStyle.RoadBrush, road.RoadTypeInfos.Width) { StartLineCap = PenLineCap.Square, EndLineCap = PenLineCap.Square });
                        }
                        dc.DrawGeometry(null, pen, CreatePath(road.Path));
                    }
                }
                var paths = MapData.Railways.Railways.Select(r => r.Path);

                DrawPaths(dc, enveloppe, GrmMapStyle.RailwayPen, paths);

                DrawAdditionals(dc, enveloppe, MapData.Additional, scale);
            }
            if (scale > 0.7)
            {
                foreach (var point in IsFalse)
                {
                    if (point.EnveloppeIntersects(enveloppe))
                    {
                        DrawAny(dc, point, GrmMapStyle.FalsePen, GrmMapStyle.FalseFill);
                    }
                }
                foreach (var point in IsTrue)
                {
                    if (point.EnveloppeIntersects(enveloppe))
                    {
                        DrawAny(dc, point, GrmMapStyle.TruePen, GrmMapStyle.TrueFill);
                    }
                }
            }
        }

        private void DrawAny(DrawingContext dc, ITerrainEnvelope geometry, Pen? pen, Brush? brush)
        {
            if (geometry is TerrainPoint point)
            {
                dc.DrawEllipse(brush, pen, ConvertToPoint(point), 3, 3);
            }
            else if ( geometry is TerrainPath path)
            {
                dc.DrawGeometry(null, pen, CreatePath(path));
            }
            else if (geometry is TerrainPolygon polygon)
            {
                dc.DrawGeometry(brush, pen, DoCreatePolygon(polygon, true));
            }
        }

        private void DrawAdditionals(DrawingContext dc, Envelope enveloppe, List<PreviewAdditionalLayer> layers, double scale)
        {
            foreach (var additional in layers)
            {
                if (additional.Polygons != null)
                {
                    DrawPolygons(dc, enveloppe, GrmMapStyle.GetAdditionalBrush(additional.Name)!, additional.Polygons, scale);
                }
                else if (additional.Paths != null)
                {
                    if (scale > 0.5)
                    {
                        DrawPaths(dc, enveloppe, GrmMapStyle.GetAdditionalPen(additional.Name)!, additional.Paths);
                    }
                }
                else if (additional.Points != null)
                {
                    if (scale > 0.7)
                    {
                        DrawPoints(dc, enveloppe, additional.Points, GrmMapStyle.GetAdditionalPen(additional.Name), GrmMapStyle.GetAdditionalBrush(additional.Name), 2);
                    }
                }
            }
        }

        private static void DrawPoints(DrawingContext dc, Envelope enveloppe, IEnumerable<TerrainPoint> points, Pen? pen, Brush? brush, int radius)
        {
            foreach (var point in points)
            {
                if (point.EnveloppeIntersects(enveloppe))
                {
                    dc.DrawEllipse(brush, pen, ConvertToPoint(point), radius, radius);
                }
            }
        }

        private void DrawPaths(DrawingContext dc, Envelope enveloppe, Pen pen, IEnumerable<TerrainPath> paths)
        {
            foreach (var path in paths)
            {
                if (path.EnveloppeIntersects(enveloppe))
                {
                    dc.DrawGeometry(null, pen, CreatePath(path));
                }
            }
        }

        private void DrawPolygons(DrawingContext dc, Envelope enveloppe, Brush brush, IEnumerable<TerrainPolygon> polygons, double scale)
        {
            var isLowScale = scale < 1;
            foreach (var poly in polygons)
            {
                if (isLowScale && poly.EnveloppeArea < 200)
                {
                    continue;
                }
                if (poly.EnveloppeIntersects(enveloppe))
                {
                    dc.DrawGeometry(brush, null, CreatePolygon(poly));
                }
            }
        }
    }
}
