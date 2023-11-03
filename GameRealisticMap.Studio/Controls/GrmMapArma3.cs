using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapArma3 : GrmMapBase
    {
        public EditableArma3Roads? Roads
        {
            get { return (EditableArma3Roads?)GetValue(RoadsProperty); }
            set { SetValue(RoadsProperty, value); }
        }

        public Dictionary<EditableArma3RoadTypeInfos, Pen> RoadBrushes { get; } = new ();

        public static readonly DependencyProperty RoadsProperty =
            DependencyProperty.Register(nameof(Roads), typeof(EditableArma3Roads), typeof(GrmMapArma3), new PropertyMetadata(null, SomePropertyChanged));

        protected override void DrawMap(DrawingContext dc, float size, Envelope enveloppe)
        {
            var roads = Roads;
            if (roads != null)
            {
                foreach (var road in roads.Roads.OrderByDescending(r => r.Order))
                {
                    if (road.Path.EnveloppeIntersects(enveloppe))
                    {
                        if (!RoadBrushes.TryGetValue(road.RoadTypeInfos, out var pen))
                        {
                            RoadBrushes.Add(road.RoadTypeInfos, pen = new Pen(GrmMapStyle.RoadBrush, road.RoadTypeInfos.TextureWidth));
                        }
                        dc.DrawGeometry(null, pen, CreatePath(size, road.Path));
                    }
                }
            }
        }
    }
}
