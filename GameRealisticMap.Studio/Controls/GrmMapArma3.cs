using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private EditablePointCollection? selectionPoints;
        private HugeImageSource<Rgb24>? source;
        private EditableArma3Road? selectedRoad;

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

        public EditableArma3Road? SelectedRoad
        {
            get { return selectedRoad; }
            set 
            {
                // ==> This should be done by ViewModel
                selectedRoad = value;
                if (selectedRoad == null)
                {
                    SelectionPoints = null;
                }
                else
                {
                    SelectionPoints = new EditablePointCollection(selectedRoad);
                }
            }
        }

        public EditablePointCollection? SelectionPoints 
        { 
            get { return selectionPoints; } 
            set 
            {
                if (selectionPoints != value)
                {
                    if (selectionPoints != null )
                    {
                        selectionPoints.CollectionChanged -= SelectionPoints_CollectionChanged;
                    }
                    selectionPoints = value;
                    if (selectionPoints != null)
                    {
                        selectionPoints.CollectionChanged += SelectionPoints_CollectionChanged;
                    }
                    CreatePoints(selectionPoints?.Points);
                }
            } 
        }

        private void SelectionPoints_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if ( e.Action == NotifyCollectionChangedAction.Add)
            {
                var terrainPoint = e.NewItems!.Cast<TerrainPoint>().Single();
                AddSquare(terrainPoint, e.NewStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                var oldPoint = e.OldItems!.Cast<TerrainPoint>().Single();
                var newPoint = e.NewItems!.Cast<TerrainPoint>().Single();
                UpdateSquare(oldPoint, newPoint, e.NewStartingIndex);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var terrainPoint = e.OldItems!.Cast<TerrainPoint>().Single();
                RemoveSquare(terrainPoint, e.OldStartingIndex);
            }
            else
            {
                CreatePoints(selectionPoints?.Points);
            }
            InvalidateVisual();
        }

        private void RemoveSquare(TerrainPoint terrainPoint, int index)
        {
            var square = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(s => s.Index == index);
            if (square != null)
            {
                InternalChildren.Remove(square);

                foreach (var child in InternalChildren.OfType<GrmMapDraggableSquare>())
                {
                    if (child.Index >= index)
                    {
                        child.Index--;
                    }
                }

                if (index == 0)
                {
                    InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(s => s.Index == index)?.Focus();
                }
                else
                {
                    InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(s => s.Index == index - 1)?.Focus();
                }
            }
        }

        private void UpdateSquare(TerrainPoint oldPoint, TerrainPoint newPoint, int index)
        {
            var square = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(s => s.Index == index);
            if (square != null && !square.TerrainPoint.Vector.Equals(newPoint.Vector))
            {
                square.TerrainPoint = newPoint;
                InvalidateArrange();
            }
        }

        private void CreatePoints(IEnumerable<TerrainPoint>? points)
        {
            InternalChildren.Clear();

            if (points != null)
            {
                var pos = 0;
                foreach (var point in points)
                {
                    InternalChildren.Add(new GrmMapDraggableSquare(this, point, pos));
                    pos++;
                }
            }
        }

        internal override void OnPointPositionPreviewChange(GrmMapDraggableSquare p)
        {
            SelectionPoints?.PreviewSet(p.Index, p.TerrainPoint);
        }

        internal override void OnPointPositionChanged(GrmMapDraggableSquare p, TerrainPoint oldValue)
        {
            SelectionPoints?.Set(p.Index, oldValue, p.TerrainPoint);
        }

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
                            RoadBrushes.Add(road.RoadTypeInfos, pen = new Pen(GrmMapStyle.RoadBrush, road.RoadTypeInfos.TextureWidth) { StartLineCap = PenLineCap.Square, EndLineCap = PenLineCap.Square });
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

            if (Keyboard.Modifiers == ModifierKeys.Control) 
            {
                OnControlClick(ViewportCoordinates(e.GetPosition(this)));
                return;
            }
            if (e.ClickCount == 2)
            {
                OnDoubleClick(ViewportCoordinates(e.GetPosition(this)));
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

                // ==> Should rise an event and let this done by ViewModel

                SelectedRoad = editRoad;
            }
            base.OnMouseLeftButtonDown(e);
        }

        private void OnDoubleClick(TerrainPoint terrainPoint)
        {
            var selectionPoints = SelectionPoints;
            if (selectionPoints != null )
            {
                var path = new TerrainPath(selectionPoints.Points.ToList());
                if ( path.Distance(terrainPoint) < 2)
                {
                    var index = path.NearestSegmentIndex(terrainPoint) + 1;
                    selectionPoints.Insert(index, terrainPoint);
                }
            }
        }

        private void OnControlClick(TerrainPoint terrainPoint)
        {
            var selectionPoints = SelectionPoints;
            var focused = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(p => p.IsFocused);
            if (focused != null && selectionPoints != null && (focused.Index == 0 || focused.Index == selectionPoints.Count - 1))
            {
                if (focused.Index == 0)
                {
                    selectionPoints.Insert(0, terrainPoint);
                }
                else
                {
                    selectionPoints.Add(terrainPoint);
                }
            }
        }

        private void AddSquare(TerrainPoint terrainPoint, int index)
        {
            foreach (var child in InternalChildren.OfType<GrmMapDraggableSquare>())
            {
                if (child.Index >= index)
                {
                    child.Index++;
                }
            }
            var item = new GrmMapDraggableSquare(this, terrainPoint, index);
            InternalChildren.Add(item);
            item.Focus();
        }

        internal override void PointPositionDelete(GrmMapDraggableSquare p)
        {
            SelectionPoints?.RemoveAt(p.Index);
        }
    }
}
