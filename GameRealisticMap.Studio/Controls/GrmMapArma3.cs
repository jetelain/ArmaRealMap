using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapArma3 : GrmMapEditMapBase
    {
        private HugeImageSource<Rgb24>? source;

        public ICommand? SelectItem
        {
            get { return (ICommand?)GetValue(SelectItemProperty); }
            set { SetValue(SelectItemProperty, value); }
        }

        public static readonly DependencyProperty SelectItemProperty =
            DependencyProperty.Register("SelectItem", typeof(ICommand), typeof(GrmMapArma3), new PropertyMetadata(null));

        public Dictionary<EditableArma3RoadTypeInfos, Pen> RoadBrushes { get; } = new();

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

        public static readonly DependencyProperty BackgroundImageResolutionProperty =
            DependencyProperty.Register("BackgroundImageResolution", typeof(double), typeof(GrmMapArma3), new PropertyMetadata(1d));


        public TerrainSpacialIndex<TerrainObjectVM>? Objects
        {
            get { return (TerrainSpacialIndex<TerrainObjectVM>)GetValue(ObjectsProperty); }
            set { SetValue(ObjectsProperty, value); }
        }

        public static readonly DependencyProperty ObjectsProperty =
            DependencyProperty.Register("Objects", typeof(TerrainSpacialIndex<TerrainObjectVM>), typeof(GrmMapArma3), new PropertyMetadata(null));

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
                    dc.PushTransform(new ScaleTransform(1 / resolution, 1 / resolution));
                }
                source.DrawTo(dc, actualClip);
                if (resolution != 1)
                {
                    dc.Pop();
                }
                dc.Pop();
            }

            dc.PushTransform(new MatrixTransform(1, 0, 0, -1, 0, size));

            var objs = Objects;
            if (objs != null)
            {
                if (Scale > 3)
                {
                    var fill = new SolidColorBrush(Color.FromArgb(192, 32, 32, 32));
                    var water = new SolidColorBrush(Color.FromArgb(192, 65, 105, 225));
                    var tree = new SolidColorBrush(Color.FromArgb(192, 34, 139, 34));
                    var bush = new SolidColorBrush(Color.FromArgb(192, 244, 164, 96));
                    foreach (var obj in objs.Search(enveloppe))
                    {
                        if (Scale * obj.Radius > 15)
                        {
                            dc.PushTransform(new MatrixTransform(obj.Matrix.M11, obj.Matrix.M13, obj.Matrix.M31, obj.Matrix.M33, obj.Matrix.M41, obj.Matrix.M43));

                            switch (obj.Category)
                            {
                                case AssetCatalogCategory.Tree:
                                    dc.DrawEllipse(tree, null, new Point(), obj.Rectangle.Width, obj.Rectangle.Height);
                                    break;
                                case AssetCatalogCategory.Bush:
                                    dc.DrawEllipse(bush, null, new Point(), obj.Rectangle.Width, obj.Rectangle.Height);
                                    break;
                                case AssetCatalogCategory.WaterSurface:
                                    dc.DrawRectangle(water, null, obj.Rectangle);
                                    break;
                                default:
                                    dc.DrawRectangle(fill, null, obj.Rectangle);
                                    break;
                            }
                            dc.Pop();
                        }
                    }
                }
            }

            var roads = Roads;
            if (roads != null)
            {
                foreach (var road in roads.Roads.OrderByDescending(r => r.Order))
                {
                    if (!road.IsRemoved && road.Path.EnveloppeIntersects(enveloppe))
                    {
                        if (!RoadBrushes.TryGetValue(road.RoadTypeInfos, out var pen))
                        {
                            RoadBrushes.Add(road.RoadTypeInfos, pen = new Pen(GrmMapStyle.RoadBrush, road.RoadTypeInfos.TextureWidth) { StartLineCap = PenLineCap.Square, EndLineCap = PenLineCap.Square });
                        }

                        dc.DrawGeometry(null, pen, CreatePath(road.Path));
                    }
                }
            }

            dc.Pop();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source == this && Keyboard.Modifiers == ModifierKeys.None && e.ClickCount == 1)
            {
                var roads = Roads;
                if (roads != null)
                {
                    var coordinates = this.ViewportCoordinates(e.GetPosition(this));
                    var enveloppe = new Envelope(coordinates - new Vector2(5), coordinates + new Vector2(5));

                    var editRoad = roads.Roads
                        .Where(r => !r.IsRemoved && r.Path.EnveloppeIntersects(enveloppe))
                        .Select(r => (road: r, distance: r.Path.Distance(coordinates)))
                        .Where(r => r.distance < r.road.RoadTypeInfos.TextureWidth / 2)
                        .OrderBy(r => r.distance)
                        .Select(r => r.road)
                        .FirstOrDefault();

                    SelectItem?.Execute(editRoad);
                }
            }
            base.OnMouseLeftButtonDown(e);
        }
    }
}
