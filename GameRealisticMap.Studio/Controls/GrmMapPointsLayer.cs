using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapPointsLayer : GrmMapDrawLayerBase
    {
        public List<TerrainPoint> Points
        {
            get { return (List<TerrainPoint>)GetValue(PointsProperty); }
            set { SetValue(PointsProperty, value); }
        }

        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points", typeof(List<TerrainPoint>), typeof(GrmMapPointsLayer), new PropertyMetadata(new List<TerrainPoint>(), SomePropertyChanged));

        public double Radius { get; set; } = 2;

        public Brush? Brush { get; set; } = new SolidColorBrush(Color.FromArgb(51, 0, 0, 0));

        public Pen? Pen { get; set; }

        protected override void DrawMap(DrawingContext dc, Envelope envelope, double scale)
        {
            foreach (var point in Points)
            {
                if (point.EnveloppeIntersects(envelope))
                {
                    dc.DrawEllipse(Brush, Pen, ConvertToPoint(point), Radius, Radius);
                }
            }
        }
    }
}
