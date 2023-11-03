using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapArma3 : GrmMapBase
    {
        private HugeImageSource<Rgb24>? source;

        public EditableArma3Roads? Roads
        {
            get { return (EditableArma3Roads?)GetValue(RoadsProperty); }
            set { SetValue(RoadsProperty, value); }
        }


        public static readonly DependencyProperty RoadsProperty =
            DependencyProperty.Register(nameof(Roads), typeof(EditableArma3Roads), typeof(GrmMapArma3), new PropertyMetadata(null, SomePropertyChanged));


        public HugeImage<Rgb24>? BackgroundImage
        {
            get { return (HugeImage<Rgb24>?)GetValue(BackgroundImageProperty); }
            set { SetValue(BackgroundImageProperty, value); }
        }

        public static readonly DependencyProperty BackgroundImageProperty =
            DependencyProperty.Register("BackgroundImage", typeof(HugeImage<Rgb24>), typeof(GrmMapArma3), new PropertyMetadata(null, SomePropertyChanged));


        public Dictionary<EditableArma3RoadTypeInfos, Pen> RoadBrushes { get; } = new();

        protected override void DrawMap(DrawingContext dc, float size, Envelope enveloppe)
        {
            var bg = BackgroundImage;
            if (bg != null)
            {
                if (source == null || source.HugeImage != bg)
                {
                    source = new HugeImageSource<Rgb24>(bg, () => Dispatcher.BeginInvoke(InvalidateVisual));
                }
                var actualClip = new Rect(DeltaX / Scale, DeltaY / Scale, ActualWidth / Scale, ActualHeight / Scale);
                dc.PushOpacity(0.8);
                source.DrawTo(dc, actualClip);
                dc.Pop();
            }

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
