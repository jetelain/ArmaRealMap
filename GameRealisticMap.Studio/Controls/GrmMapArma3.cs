using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels;
using GameRealisticMap.Studio.Modules.AssetBrowser.Services;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapArma3 : GrmMapDrawLayerBase
    {
        public static readonly DependencyProperty SelectItemProperty =
            DependencyProperty.Register("SelectItem", typeof(ICommand), typeof(GrmMapArma3), new PropertyMetadata(null));

        public static readonly DependencyProperty AddToSelectionCommandProperty =
            DependencyProperty.Register("AddToSelectionCommand", typeof(ICommand), typeof(GrmMapArma3), new PropertyMetadata(null));

        public ICommand? SelectItem
        {
            get { return (ICommand?)GetValue(SelectItemProperty); }
            set { SetValue(SelectItemProperty, value); }
        }

        public ICommand? AddToSelectionCommand
        {
            get { return (ICommand?)GetValue(AddToSelectionCommandProperty); }
            set { SetValue(AddToSelectionCommandProperty, value); }
        }

        public Dictionary<EditableArma3RoadTypeInfos, Pen> RoadBrushes { get; } = new();

        public EditableArma3Roads? Roads
        {
            get { return (EditableArma3Roads?)GetValue(RoadsProperty); }
            set { SetValue(RoadsProperty, value); }
        }

        public static readonly DependencyProperty RoadsProperty =
            DependencyProperty.Register(nameof(Roads), typeof(EditableArma3Roads), typeof(GrmMapArma3), new PropertyMetadata(null, SomePropertyChanged));

        public TerrainSpacialIndex<TerrainObjectVM>? Objects
        {
            get { return (TerrainSpacialIndex<TerrainObjectVM>)GetValue(ObjectsProperty); }
            set { SetValue(ObjectsProperty, value); }
        }

        public static readonly DependencyProperty ObjectsProperty =
            DependencyProperty.Register("Objects", typeof(TerrainSpacialIndex<TerrainObjectVM>), typeof(GrmMapArma3), new PropertyMetadata(null, SomePropertyChanged));


        public AerialViewMode AerialViewMode
        {
            get { return (AerialViewMode)GetValue(AerialViewModeProperty); }
            set { SetValue(AerialViewModeProperty, value); }
        }

        public static readonly DependencyProperty AerialViewModeProperty =
            DependencyProperty.Register(nameof(AerialViewMode), typeof(AerialViewMode), typeof(GrmMapArma3), new PropertyMetadata(AerialViewMode.Shape, SomePropertyChanged));

        private SolidColorBrush fill = new SolidColorBrush(Color.FromArgb(192, 32, 32, 32));
        private SolidColorBrush water = new SolidColorBrush(Color.FromArgb(192, 65, 105, 225));
        private SolidColorBrush tree = new SolidColorBrush(Color.FromArgb(192, 34, 139, 34));
        private SolidColorBrush bush = new SolidColorBrush(Color.FromArgb(192, 244, 164, 96));

        protected override void DrawMap(DrawingContext dc, Envelope enveloppe, double scale)
        {
            var objs = Objects;
            if (objs != null)
            {
                if (scale > 3)
                {
                    var mode = AerialViewMode;
                    foreach (var obj in objs.Search(enveloppe))
                    {
                        if (scale * obj.Radius > 15)
                        {
                            dc.PushTransform(obj.Matrix.ToAerialWpfMatrixTransform());

                            RenderObject(dc, mode, obj);

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
        }

        private void RenderObject(DrawingContext dc, AerialViewMode mode, TerrainObjectVM obj)
        {
            if (mode == Controls.AerialViewMode.Shape)
            {
                FillShape(dc, obj);
            }
            else
            {
                var img = IoC.Get<IArma3AerialImageService>().GetImage(obj.Model); // TODO: Create an option to opt-in/opt-out
                if (img != null)
                {
                    dc.DrawImage(img, new Rect(-img.PixelWidth / 8, -img.PixelHeight / 8, img.PixelWidth / 4, img.PixelHeight / 4));
                    if (mode == Controls.AerialViewMode.ShapeAndImage)
                    {
                        OutlineShape(dc, obj);
                    }
                }
                else
                {
                    FillShape(dc, obj);
                }
            }
        }

        private void OutlineShape(DrawingContext dc, TerrainObjectVM obj)
        {
            switch (obj.Category)
            {
                case AssetCatalogCategory.Tree:
                    dc.DrawEllipse(null, new Pen(tree, 0.2), new Point(), obj.Rectangle.Width, obj.Rectangle.Height);
                    break;
                case AssetCatalogCategory.Bush:
                    dc.DrawEllipse(null, new Pen(bush, 0.2), new Point(), obj.Rectangle.Width, obj.Rectangle.Height);
                    break;
                case AssetCatalogCategory.WaterSurface:
                    dc.DrawRectangle(null, new Pen(water, 0.2), obj.Rectangle);
                    break;
                default:
                    dc.DrawRectangle(null, new Pen(fill, 0.2), obj.Rectangle);
                    break;
            }
        }

        private void FillShape(DrawingContext dc, TerrainObjectVM obj)
        {
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
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source == this && e.ClickCount == 1)
            {
                var roads = Roads;
                if (roads != null)
                {
                    var coordinates = ParentMap!.ViewportCoordinates(e.GetPosition(ParentMap));
                    var enveloppe = new Envelope(coordinates - new Vector2(5), coordinates + new Vector2(5));

                    var editRoad = roads.Roads
                        .Where(r => !r.IsRemoved && r.Path.EnveloppeIntersects(enveloppe))
                        .Select(r => (road: r, distance: r.Path.Distance(coordinates)))
                        .Where(r => r.distance < r.road.RoadTypeInfos.TextureWidth / 2)
                        .OrderBy(r => r.distance)
                        .Select(r => r.road)
                        .FirstOrDefault();

                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        AddToSelectionCommand?.Execute(editRoad);
                    }
                    else
                    {
                        SelectItem?.Execute(editRoad);
                    }
                    e.Handled = true;
                }
            }
            base.OnMouseLeftButtonDown(e);
        }
    }
}
