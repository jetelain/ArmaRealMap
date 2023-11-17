using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Behaviors;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapEditLayer : Panel, IGrmMapLayer
    {
        public static readonly DependencyProperty ClearSelectionProperty =
            DependencyProperty.Register(nameof(ClearSelection), typeof(ICommand), typeof(GrmMapEditLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty EditPointsProperty =
            DependencyProperty.Register(nameof(EditPoints), typeof(IEditablePointCollection), typeof(GrmMapEditLayer), new PropertyMetadata(null, EditPoints_Changed));

        private readonly GrmMapEditLayerOverlay overlay;
        private GrmMap? parentMap;
        private IEditablePointCollection? editPoints;
        private bool isPointInsertPreview;
        private Point previewInsertPoint = new Point();
        private bool isPreviewEnd;

        public GrmMapEditLayer()
        {
            overlay = new GrmMapEditLayerOverlay(this);
        }

        public IEditablePointCollection? EditPoints
        {
            get { return (IEditablePointCollection?)GetValue(EditPointsProperty); }
            set { SetValue(EditPointsProperty, value); }
        }

        public GrmMap? ParentMap 
        { 
            get { return parentMap; }
            set 
            { 
                parentMap = value; 
                foreach(var layer in InternalChildren.OfType<IGrmMapLayer>())
                {
                    layer.ParentMap = parentMap;
                }
            }
        }

        public ICommand? ClearSelection
        {
            get { return (ICommand?)GetValue(ClearSelectionProperty); }
            set { SetValue(ClearSelectionProperty, value); }
        }

        private static void EditPoints_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GrmMapEditLayer)d).ChangeEditPoints((IEditablePointCollection?)e.OldValue, (IEditablePointCollection?)e.NewValue);
        }

        private void ChangeEditPoints(IEditablePointCollection? oldValue, IEditablePointCollection? newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= SelectionPoints_CollectionChanged;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += SelectionPoints_CollectionChanged;
            }
            editPoints = newValue;
            CreatePoints(newValue);
            overlay.InvalidateVisual();
        }

        private void SelectionPoints_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
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
                CreatePoints(editPoints);
            }
            OnViewportChanged();
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
            if (!InternalChildren.Contains(overlay))
            {
                InternalChildren.Add(overlay);
            }
            var first = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault();
            if (first != null)
            {
                var index = InternalChildren.IndexOf(first);
                InternalChildren.RemoveRange(index, InternalChildren.Count - index);
            }
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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            var parent = ParentMap;
            if (parent != null)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    OnControlClick(parent.ViewportCoordinates(e.GetPosition(parent)), e);
                    return;
                }
                if (e.ClickCount == 2)
                {
                    OnDoubleClick(parent.ViewportCoordinates(e.GetPosition(parent)), e);
                    return;
                }
            }
            ClearSelection?.Execute(null);
            base.OnMouseLeftButtonDown(e);
        }

        private void OnDoubleClick(TerrainPoint terrainPoint, MouseButtonEventArgs e)
        {
            var selectionPoints = editPoints;
            if (selectionPoints != null && selectionPoints.CanInsertBetween)
            {
                var path = new TerrainPath(selectionPoints.ToList());
                if (path.Distance(terrainPoint) < 2)
                {
                    var index = path.NearestSegmentIndex(terrainPoint) + 1;
                    selectionPoints.Insert(index, terrainPoint);
                    e.Handled = true;
                }
            }
        }

        private void OnControlClick(TerrainPoint terrainPoint, MouseButtonEventArgs e)
        {
            var selectionPoints = editPoints;
            if (selectionPoints != null && selectionPoints.CanInsertAtEnds)
            {
                var focused = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(p => p.IsFocused);
                if (focused != null && (focused.Index == 0 || focused.Index == selectionPoints.Count - 1))
                {
                    if (focused.Index == 0)
                    {
                        selectionPoints.Insert(0, terrainPoint);
                    }
                    else
                    {
                        selectionPoints.Add(terrainPoint);
                    }
                    e.Handled = true;
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

        internal void OnPointPositionPreviewChange(GrmMapDraggableSquare p)
        {
            editPoints?.PreviewSet(p.Index, p.TerrainPoint);
        }

        internal void OnPointPositionChanged(GrmMapDraggableSquare p, TerrainPoint oldValue)
        {
            editPoints?.Set(p.Index, oldValue, p.TerrainPoint);
        }

        internal void PointPositionDelete(GrmMapDraggableSquare p)
        {
            editPoints?.RemoveAt(p.Index);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var rect = new Rect(new Point(), RenderSize);
            var map = ParentMap;
            if (map != null)
            {
                foreach (UIElement internalChild in InternalChildren)
                {
                    if (internalChild is GrmMapDraggableSquare square)
                    {
                        var s = internalChild.DesiredSize;
                        var p = map.ProjectViewport(square.TerrainPoint) - new System.Windows.Vector(s.Width / 2, s.Height / 2);
                        internalChild.Arrange(new Rect(p, s));
                    }
                    else
                    {
                        internalChild.Arrange(rect);
                    }
                }
            }
            return arrangeSize;
        }


        protected override Size MeasureOverride(Size constraint)
        {
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement internalChild in base.InternalChildren)
            {
                internalChild?.Measure(availableSize);
            }
            return default(Size);
        }

        internal void OpenItemContextMenu(GrmMapDraggableSquare square, MouseButtonEventArgs e)
        {
            if (editPoints != null)
            {
                var menu = new ContextMenu(); // To do on Xaml side ?
                menu.Items.Add(new MenuItem()
                {
                    Header = "Split into two paths",
                    IsEnabled = editPoints.CanSplit && square.Index > 0 && square.Index < editPoints.Count - 1,
                    Command = new RelayCommand(_ => editPoints.SplitAt(square.Index))
                });
                menu.Items.Add(new MenuItem()
                {
                    Header = "Delete point",
                    Command = new RelayCommand(_ => PointPositionDelete(square))
                });
                ButtonBehaviors.ShowButtonContextMenu(square, menu);
            }
        }

        internal PathGeometry? CreateEditPointGeometry()
        {
            if (editPoints != null)
            {
                return CreateEditPointGeometry(editPoints);
            }
            return null;
        }

        private PathGeometry CreateEditPointGeometry(IEditablePointCollection source)
        {
            var points = source.Select(p => ParentMap!.ProjectViewport(p)).ToList();
            if (isPointInsertPreview)
            {
                if (isPreviewEnd)
                { 
                    points.Add(previewInsertPoint); 
                }
                else
                {
                    points.Insert(0, previewInsertPoint);
                }
            }
            var path = new PathGeometry();
            var figure = new PathFigure { StartPoint = points.First() };
            figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
            path.Figures.Add(figure);
            return path;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            UpdatePreviewState();
        }

        private void UpdatePreviewState()
        {
            var isControl = Keyboard.Modifiers == ModifierKeys.Control;
            if (isPointInsertPreview != isControl)
            {
                var isInsertMode = false;
                if ( isControl && editPoints != null && CanInsert(editPoints, out var focused))
                {
                    isPreviewEnd = focused.Index != 0;
                    isInsertMode = true;
                }
                if (isPointInsertPreview != isInsertMode)
                {
                    isPointInsertPreview = isInsertMode;
                    previewInsertPoint = Mouse.GetPosition(this);
                    overlay.InvalidateVisual();
                }
            }
        }

        private bool CanInsert(IEditablePointCollection selectionPoints, [NotNullWhen(true)] out GrmMapDraggableSquare? focused)
        {
            focused = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(p => p.IsFocused);
            return focused != null && (focused.Index == 0 || focused.Index == selectionPoints.Count - 1);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            UpdatePreviewState();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isPointInsertPreview)
            {
                previewInsertPoint = e.GetPosition(this);
                overlay.InvalidateVisual();
            }
        }

        public void OnViewportChanged()
        {
            foreach (var layer in InternalChildren.OfType<IGrmMapLayer>())
            {
                layer.OnViewportChanged();
            }
            InvalidateVisual();
            InvalidateArrange();
            overlay.InvalidateVisual();
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            if (visualAdded is IGrmMapLayer layerAdded)
            {
                layerAdded.ParentMap = parentMap;
            }
            if (visualRemoved is IGrmMapLayer layerRemoved)
            {
                layerRemoved.ParentMap = parentMap;
            }
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
