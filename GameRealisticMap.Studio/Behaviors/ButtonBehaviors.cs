using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GameRealisticMap.Studio.Behaviors
{
    internal static class ButtonBehaviors
    {
        public static void SetOpenContextOnClick(ButtonBase button, bool value)
        {
            if (value)
            {
                button.Click += Button_Click;
            }
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ButtonBase;
            if (button != null)
            {
                var context = button.ContextMenu;
                if (context != null)
                {
                    ShowButtonContextMenu(button, context);
                }
            }
        }

        internal static void ShowButtonContextMenu(FrameworkElement button, ContextMenu context)
        {
            context.Placement = PlacementMode.Relative;
            context.PlacementTarget = button;
            context.HorizontalOffset = button.Margin.Left;
            context.VerticalOffset = button.ActualHeight - button.Margin.Bottom;
            context.IsOpen = true;
        }
    }
}
