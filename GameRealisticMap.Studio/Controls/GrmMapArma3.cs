using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
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



        public double BackgroundImageResolution
        {
            get { return (double)GetValue(BackgroundImageResolutionProperty); }
            set { SetValue(BackgroundImageResolutionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BackgroundImageResolution.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundImageResolutionProperty =
            DependencyProperty.Register("BackgroundImageResolution", typeof(double), typeof(GrmMapArma3), new PropertyMetadata(1d));




        public Dictionary<EditableArma3RoadTypeInfos, Pen> RoadBrushes { get; } = new();

        protected override void DrawMap(DrawingContext dc, float size, Envelope enveloppe)
        {
            var bg = BackgroundImage;
            if (bg != null)
            {
                if (source == null || source.HugeImage != bg)
                {
                    source = new HugeImageSource<Rgb24>(bg, InvalidateVisual);
                }
                var resolution = BackgroundImageResolution;
                var renderSize = RenderSize;

                var actualClip = new Rect(DeltaX / Scale * resolution, DeltaY / Scale * resolution, renderSize.Width / Scale * resolution, renderSize.Height / Scale * resolution);
                dc.PushOpacity(0.8);
                if (resolution != 1)
                {
                    dc.PushTransform(new ScaleTransform(1/ resolution, 1/ resolution));
                }
                source.DrawTo(dc, actualClip);
                if (resolution != 1)
                {
                    dc.Pop();
                }
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
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source != this)
            {
                return;
            }

            

            var roads = Roads;
            if (roads != null)
            {
                var coordinates = this.ViewportCoordinates(e.GetPosition(this));
                var enveloppe = new Envelope(coordinates - new Vector2(5), coordinates + new Vector2(5));

                var editRoad = roads.Roads
                    .Where(r => r.Path.EnveloppeIntersects(enveloppe))
                    .Select(r => (road: r, distance: r.Path.Distance(coordinates)))
                    .Where(r => r.distance < r.road.RoadTypeInfos.TextureWidth / 2)
                    .OrderBy(r => r.distance)
                    .Select(r => r.road)
                    .FirstOrDefault();

                if (editRoad != null)
                {
                    InternalChildren.Clear();
                    var pos = 0;
                    foreach (var point in editRoad.Path.Points)
                    {
                        var idx = pos;
                        var p = new GrmMapDraggableSquare() { TerrainPoint = point };
                        p.Edited += (_,_) => {
                            var points = editRoad.Path.Points.ToList();
                            points[idx] = p.TerrainPoint;
                            editRoad.Path = new TerrainPath(points);
                            InvalidateVisual();
                        };
                        InternalChildren.Add(p);
                        pos++;
                    }
                }
            }
            base.OnMouseLeftButtonDown(e);
        }


    }
}
