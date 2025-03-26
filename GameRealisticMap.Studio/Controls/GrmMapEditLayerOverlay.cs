using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Studio.Controls
{
    internal sealed class GrmMapEditLayerOverlay : FrameworkElement
    {
        private readonly GrmMapEditLayer owner;

        private readonly Pen pen = new Pen(new SolidColorBrush(Colors.White), 2) { DashStyle = DashStyles.Dot };
        private readonly Brush brush = new SolidColorBrush(Colors.White);

        public GrmMapEditLayerOverlay(GrmMapEditLayer owner)
        {
            this.owner = owner;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var parentMap = owner.ParentMap;
            if (parentMap != null)
            {
                var outline = owner.Outline;
                if (outline != null)
                {
                    RenderOutline(dc, parentMap, outline);
                }
                else
                {
                    var editPoints = owner.EditPoints;
                    if (editPoints != null && editPoints.Count > 0)
                    {
                        dc.DrawGeometry(null, pen, CreateEditPointGeometry(parentMap, editPoints));
                    }
                }

            }
        }

        private void RenderOutline(DrawingContext dc, GrmMap parentMap, IEnumerable<IReadOnlyCollection<TerrainPoint>> outline)
        {
            foreach (var segment in outline)
            {
                if (segment.Count > 0)
                {
                    var points = segment.Select(p => parentMap.ProjectViewport(p)).ToList();

                    if (segment is IEditablePointCollection a && a.IsObjectSquare)
                    {
                        foreach (var point in points)
                        {
                            dc.DrawRectangle(brush, null, new Rect(((Point)(point - new Point(3, 3))), new Size(6, 6)));
                        }
                        if (points.Count == 5)
                        {
                            points.RemoveAt(4);
                        }
                        points.Add(points[0]);
                        dc.DrawGeometry(null, pen, CreateGeometry(points));
                      }
                    else
                    {
                        dc.DrawGeometry(null, pen, CreateGeometry(points));
                        dc.DrawRectangle(brush, null, new Rect(((Point)(points[0] - new Point(3, 3))), new Size(6, 6)));
                        dc.DrawRectangle(brush, null, new Rect(((Point)(points[points.Count - 1] - new Point(3, 3))), new Size(6, 6)));
                    }
                }
            }
        }

        private PathGeometry CreateEditPointGeometry(GrmMap parentMap, IEditablePointCollection source)
        {
            var points = source.Select(p => parentMap.ProjectViewport(p)).ToList();
            if (owner.EditMode == GrmMapEditMode.ContinuePath)
            {
                var previewInsertPoint = Mouse.GetPosition(parentMap);
                if (owner.IsPreviewEnd)
                {
                    points.Add(previewInsertPoint);
                }
                else
                {
                    points.Insert(0, previewInsertPoint);
                }
            }
            if (source.IsObjectSquare)
            {
                if (points.Count == 5)
                {
                    points.RemoveAt(4);
                }
                points.Add(points[0]);
            }
            return CreateGeometry(points);
        }

        private static PathGeometry CreateGeometry(IEnumerable<Point> points)
        {
            var path = new PathGeometry();
            var figure = new PathFigure { StartPoint = points.First() };
            figure.Segments.Add(new PolyLineSegment(points.Skip(1), true));
            path.Figures.Add(figure);
            return path;
        }
    }
}
