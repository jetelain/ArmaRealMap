using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Studio.Shared;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapViewer : GrmMapBase
    {
        public GrmMapViewer()
        {

        }

        public Dictionary<RoadTypeId,Pen> RoadBrushes { get; } = new Dictionary<RoadTypeId,Pen>();

        public List<ITerrainEnvelope> IsTrue
        {
            get { return (List<ITerrainEnvelope>)GetValue(IsTrueProperty); }
            set { SetValue(IsTrueProperty, value); }
        }

        public static readonly DependencyProperty IsTrueProperty =
            DependencyProperty.Register(nameof(IsTrue), typeof(List<ITerrainEnvelope>), typeof(GrmMapViewer), new PropertyMetadata(new List<ITerrainEnvelope>(), SomePropertyChanged));


        public List<ITerrainEnvelope> IsFalse
        {
            get { return (List<ITerrainEnvelope>)GetValue(FalseListProperty); }
            set { SetValue(FalseListProperty, value); }
        }
        public static readonly DependencyProperty FalseListProperty =
            DependencyProperty.Register(nameof(IsFalse), typeof(List<ITerrainEnvelope>), typeof(GrmMapViewer), new PropertyMetadata(new List<ITerrainEnvelope>(), SomePropertyChanged));

        public PreviewMapData? MapData
        {
            get { return (PreviewMapData?)GetValue(MapDataProperty); }
            set { SetValue(MapDataProperty, value); }
        }

        public static readonly DependencyProperty MapDataProperty =
            DependencyProperty.Register(nameof(MapData), typeof(PreviewMapData), typeof(GrmMapViewer), new PropertyMetadata(null, SomePropertyChanged));

        protected override void DrawMap(DrawingContext dc, float size, Envelope enveloppe)
        {
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
                        DrawAny(dc, size, point, GrmMapStyle.FalsePen, GrmMapStyle.FalseFill);
                    }
                }
                foreach (var point in IsTrue)
                {
                    if (point.EnveloppeIntersects(enveloppe))
                    {
                        DrawAny(dc, size, point, GrmMapStyle.TruePen, GrmMapStyle.TrueFill);
                    }
                }
            }
        }

        private void DrawAny(DrawingContext dc, float size, ITerrainEnvelope geometry, Pen? pen, Brush? brush)
        {
            if (geometry is TerrainPoint point)
            {
                dc.DrawEllipse(brush, pen, Convert(point, size), 3, 3);
            }
            else if ( geometry is TerrainPath path)
            {
                dc.DrawGeometry(null, pen, CreatePath(size, path));
            }
            else if (geometry is TerrainPolygon polygon)
            {
                dc.DrawGeometry(brush, pen, DoCreatePolygon(size, polygon, true));
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
    }
}
