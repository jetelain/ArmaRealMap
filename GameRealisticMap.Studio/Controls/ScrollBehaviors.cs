using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace GameRealisticMap.Studio.Controls
{
    public static class ScrollBehaviors
    {
        public static void SetWheelBubble(FrameworkElement element, bool value)
        {
            if (value)
            {
                element.Loaded += Loaded;
            }
        }

        private static void Loaded(object sender, RoutedEventArgs loadEvent)
        {
            if (sender is ScrollViewer sv)
            {
                SetupWheelBubble((UIElement)sv.Parent, sv);
            }
            else
            {
                var owner = (FrameworkElement)sender;
                var parent = ((UIElement)owner.Parent);
                var childSV = VisualTreeHelperEx.FindDescendantByType<ScrollViewer>(owner);
                if (childSV != null)
                {
                    SetupWheelBubble(parent, childSV);
                }
            }
        }

        private static void SetupWheelBubble(UIElement eventTarget, ScrollViewer scrollViewer)
        {
            scrollViewer.PreviewMouseWheel += (_, previewSrollEvent) =>
            {
                if (scrollViewer.ScrollableHeight == 0
                    || (previewSrollEvent.Delta > 0 && scrollViewer.VerticalOffset == 0)
                    || (previewSrollEvent.Delta < 0 && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight))
                {
                    previewSrollEvent.Handled = true;

                    var scrollEvent = new MouseWheelEventArgs(previewSrollEvent.MouseDevice, previewSrollEvent.Timestamp, previewSrollEvent.Delta);
                    scrollEvent.RoutedEvent = UIElement.MouseWheelEvent;
                    eventTarget.RaiseEvent(scrollEvent);
                }
            };
        }
    }
}
