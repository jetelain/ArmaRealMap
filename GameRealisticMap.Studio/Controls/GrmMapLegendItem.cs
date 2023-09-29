using System.Windows;
using System.Windows.Media;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapLegendItem : FrameworkElement
    {
        public GrmMapLegendItem()
        {
            Width = 20;
            Height = 20;
        }

        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }

        public static readonly DependencyProperty ItemNameProperty =
            DependencyProperty.Register("ItemName", typeof(string), typeof(GrmMapLegendItem), new PropertyMetadata("Tree", SomePropertyChanged));

        public LegendItemType ItemType
        {
            get { return (LegendItemType)GetValue(ItemTypeProperty); }
            set { SetValue(ItemTypeProperty, value); }
        }

        public static readonly DependencyProperty ItemTypeProperty =
            DependencyProperty.Register("ItemType", typeof(LegendItemType), typeof(GrmMapLegendItem), new PropertyMetadata(LegendItemType.Point, SomePropertyChanged));


        private static void SomePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GrmMapLegendItem)?.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {

            dc.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(0, 0, 20, 20));

            switch (ItemType)
            {
                case LegendItemType.Point:
                    dc.DrawEllipse(GrmMapStyle.GetAdditionalBrush(ItemName), GrmMapStyle.GetAdditionalPen(ItemName), new Point(10, 10), 2, 2);
                    break;

                case LegendItemType.Path:
                    dc.PushClip(new RectangleGeometry(new Rect(0, 0, 20, 20)));
                    dc.DrawLine(GrmMapStyle.GetAdditionalPen(ItemName), new Point(0, 10), new Point(20, 10));
                    dc.Pop();
                    break;

                case LegendItemType.Polygon:
                    dc.DrawRectangle(GrmMapStyle.GetAdditionalBrush(ItemName), null, new Rect(0, 0, 20, 20));
                    break;
            }
        }

    }
}
