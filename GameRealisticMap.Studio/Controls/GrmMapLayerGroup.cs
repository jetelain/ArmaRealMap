using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GameRealisticMap.Studio.Controls
{
    public class GrmMapLayerGroup : Panel, IGrmMapLayer
    {
        protected GrmMap? parentMap;

        public GrmMap? ParentMap
        {
            get { return parentMap; }
            set
            {
                parentMap = value;
                foreach (var layer in InternalChildren.OfType<IGrmMapLayer>())
                {
                    layer.ParentMap = parentMap;
                }
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var rect = new Rect(new Point(), RenderSize);
            foreach (UIElement internalChild in InternalChildren)
            {
                internalChild?.Arrange(rect);
            }
            return arrangeSize;
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

        protected override Size MeasureOverride(Size constraint)
        {
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            foreach (UIElement internalChild in base.InternalChildren)
            {
                internalChild?.Measure(availableSize);
            }
            return default(Size);
        }

        public virtual void OnViewportChanged()
        {
            foreach (var layer in InternalChildren.OfType<IGrmMapLayer>())
            {
                layer.OnViewportChanged();
            }
        }
    }
}
