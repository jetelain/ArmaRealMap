using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    public abstract class GrmMapEditMapBase : GrmMapBase
    {
        private IEditablePointCollection? editPoints;

        public IEditablePointCollection? EditPoints
        {
            get { return (IEditablePointCollection?)GetValue(EditPointsProperty); }
            set { SetValue(EditPointsProperty, value); }
        }

        public static readonly DependencyProperty EditPointsProperty =
            DependencyProperty.Register("EditPoints", typeof(IEditablePointCollection), typeof(GrmMapEditMapBase), new PropertyMetadata(null, EditPoints_Changed));

        private static void EditPoints_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GrmMapEditMapBase)d).ChangeEditPoints((IEditablePointCollection?)e.OldValue, (IEditablePointCollection?)e.NewValue);
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
                CreatePoints(editPoints);
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



        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.Source != this)
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control) 
            {
                OnControlClick(ViewportCoordinates(e.GetPosition(this)), e);
                return;
            }
            if (e.ClickCount == 2)
            {
                OnDoubleClick(ViewportCoordinates(e.GetPosition(this)), e);
                return;
            }

            base.OnMouseLeftButtonDown(e);
        }

        private void OnDoubleClick(TerrainPoint terrainPoint, MouseButtonEventArgs e)
        {
            var selectionPoints = editPoints;
            if (selectionPoints != null && selectionPoints.CanInsertBetween)
            {
                var path = new TerrainPath(selectionPoints.ToList());
                if ( path.Distance(terrainPoint) < 2)
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
            var size = SizeInMeters;
            foreach (UIElement internalChild in InternalChildren)
            {
                if (internalChild is GrmMapDraggableSquare square)
                {
                    var s = internalChild.DesiredSize;
                    var p = Translate.Transform(ScaleTr.Transform(Convert(square.TerrainPoint, size))) - new System.Windows.Vector(s.Width / 2, s.Height / 2);
                    internalChild.Arrange(new Rect(p, s));
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
    }
}
