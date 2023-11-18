using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Behaviors;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapEditLayer : GrmMapLayerGroup
    {
        public static readonly DependencyProperty ClearSelectionProperty =
            DependencyProperty.Register(nameof(ClearSelection), typeof(ICommand), typeof(GrmMapEditLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty InsertPointCommandProperty =
            DependencyProperty.Register(nameof(InsertPointCommand), typeof(ICommand), typeof(GrmMapEditLayer), new PropertyMetadata(null));

        public static readonly DependencyProperty EditPointsProperty =
            DependencyProperty.Register(nameof(EditPoints), typeof(IEditablePointCollection), typeof(GrmMapEditLayer), new PropertyMetadata(null, EditPoints_Changed));

        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register(nameof(EditMode), typeof(GrmMapEditMode), typeof(GrmMapEditLayer), new PropertyMetadata(GrmMapEditMode.None, EditMode_Changed));

        private readonly GrmMapEditLayerOverlay overlay;
        private IEditablePointCollection? editPoints;
        private bool isPreviewEnd;

        public GrmMapEditLayer()
        {
            overlay = new GrmMapEditLayerOverlay(this);
        }

        public GrmMapEditMode EditMode
        {
            get { return (GrmMapEditMode)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        public IEditablePointCollection? EditPoints
        {
            get { return (IEditablePointCollection?)GetValue(EditPointsProperty); }
            set { SetValue(EditPointsProperty, value); }
        }

        public ICommand? ClearSelection
        {
            get { return (ICommand?)GetValue(ClearSelectionProperty); }
            set { SetValue(ClearSelectionProperty, value); }
        }

        public ICommand? InsertPointCommand
        {
            get { return (ICommand?)GetValue(InsertPointCommandProperty); }
            set { SetValue(InsertPointCommandProperty, value); }
        }

        private static void EditPoints_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GrmMapEditLayer)d).ChangeEditPoints((IEditablePointCollection?)e.OldValue, (IEditablePointCollection?)e.NewValue);
        }

        private static void EditMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GrmMapEditLayer)d).ChangeEditMode((GrmMapEditMode)e.OldValue, (GrmMapEditMode)e.NewValue);
        }

        private void ChangeEditMode(GrmMapEditMode oldValue, GrmMapEditMode newValue)
        {
            if (newValue == GrmMapEditMode.None)
            {
                Cursor = null;
            }
            else
            {
                Cursor = Cursors.Cross;

                if (newValue == GrmMapEditMode.ContinuePath)
                {
                    if (!InternalChildren.OfType<GrmMapDraggableSquare>().Any(p => p.IsFocused))
                    {
                        InternalChildren.OfType<GrmMapDraggableSquare>().First(p => p.Index == 0).Focus();
                    }
                }
            }
            overlay.InvalidateVisual();
        }

        private void ChangeEditPoints(IEditablePointCollection? oldValue, IEditablePointCollection? newValue)
        {
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= SelectionPoints_CollectionChanged;
                oldValue.PropertyChanged -= SelectionPoints_PropertyChanged;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += SelectionPoints_CollectionChanged;
                newValue.PropertyChanged += SelectionPoints_PropertyChanged;
            }
            editPoints = newValue;
            CreatePoints(newValue);
            overlay.InvalidateVisual();
        }

        private void SelectionPoints_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnViewportChanged();
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
                var mode = EditMode;
                if (mode == GrmMapEditMode.InsertPoint)
                {
                    InsertPointCommand?.Execute(parent.ViewportCoordinates(e.GetPosition(parent)));
                    e.Handled = true;
                    return;
                }
                if (mode == GrmMapEditMode.ContinuePath)
                {
                    ContinuePath(parent.ViewportCoordinates(e.GetPosition(parent)), e);
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

        private void ContinuePath(TerrainPoint terrainPoint, MouseButtonEventArgs e)
        {
            if (editPoints != null && CanInsert(editPoints, out var focused))
            {
                if (focused.Index == 0)
                {
                    editPoints.Insert(0, terrainPoint);
                }
                else
                {
                    editPoints.Add(terrainPoint);
                }
                e.Handled = true;
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

        internal void OpenItemContextMenu(GrmMapDraggableSquare square, MouseButtonEventArgs e)
        {
            if (editPoints != null)
            {
                var menu = new ContextMenu(); // To do on Xaml side ?
                if (editPoints.CanSplit)
                {
                    menu.Items.Add(new MenuItem()
                    {
                        Header = "Split into two paths",
                        Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Tools/split.png")) },
                        IsEnabled = square.Index > 0 && square.Index < editPoints.Count - 1,
                        Command = new RelayCommand(_ => editPoints.SplitAt(square.Index))
                    });
                }
                if (editPoints.CanInsertAtEnds)
                {
                    menu.Items.Add(new MenuItem()
                    {
                        Header = "Continue path",
                        Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Tools/path.png")) },
                        IsEnabled = square.Index == 0 || square.Index == editPoints.Count - 1,
                        Command = new RelayCommand(_ => ContinuePathFrom(square))
                    });
                }
                menu.Items.Add(new MenuItem()
                {
                    Header = "Delete point",
                    Icon = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Tools/bin.png")) },
                    Command = new RelayCommand(_ => PointPositionDelete(square))
                });
                ButtonBehaviors.ShowButtonContextMenu(square, menu);
            }
        }

        internal PathGeometry? CreateEditPointGeometry()
        {
            if (editPoints != null && editPoints.Count > 0)
            {
                return CreateEditPointGeometry(editPoints);
            }
            return null;
        }

        private PathGeometry CreateEditPointGeometry(IEditablePointCollection source)
        {
            var points = source.Select(p => ParentMap!.ProjectViewport(p)).ToList();
            if (EditMode == GrmMapEditMode.ContinuePath)
            {
                var previewInsertPoint = Mouse.GetPosition(parentMap);
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
            UpdatePreviewState(e.Key);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            UpdatePreviewState(e.Key);
        }

        private void UpdatePreviewState(Key key)
        {
            if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                var currentState = EditMode;
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    if (currentState != GrmMapEditMode.ContinuePath && editPoints != null && CanInsert(editPoints, out var focused))
                    {
                        ContinuePathFrom(focused);
                    }
                }
                else
                {
                    if (currentState != GrmMapEditMode.None)
                    {
                        EditMode = GrmMapEditMode.None;
                    }
                }
            }
        }

        private void ContinuePathFrom(GrmMapDraggableSquare focused)
        {
            isPreviewEnd = focused.Index != 0;
            EditMode = GrmMapEditMode.ContinuePath;
        }

        private bool CanInsert(IEditablePointCollection selectionPoints, [NotNullWhen(true)] out GrmMapDraggableSquare? focused)
        {
            if (selectionPoints.CanInsertAtEnds)
            {
                focused = InternalChildren.OfType<GrmMapDraggableSquare>().FirstOrDefault(p => p.IsFocused);
                return focused != null && (focused.Index == 0 || focused.Index == selectionPoints.Count - 1);
            }
            focused = null;
            return false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (EditMode == GrmMapEditMode.ContinuePath)
            {
                overlay.InvalidateVisual();
            }
        }

        public override void OnViewportChanged()
        {
            base.OnViewportChanged();
            InvalidateArrange();
            overlay.InvalidateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }
}
