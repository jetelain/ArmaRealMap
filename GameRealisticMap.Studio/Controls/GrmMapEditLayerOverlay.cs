using System.Windows;
using System.Windows.Media;

namespace GameRealisticMap.Studio.Controls
{
    internal sealed class GrmMapEditLayerOverlay : FrameworkElement
    {
        private readonly GrmMapEditLayer owner;

        public GrmMapEditLayerOverlay(GrmMapEditLayer owner)
        {
            this.owner = owner;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var geometry = owner.CreateEditPointGeometry();
            if (geometry != null)
            {
                dc.DrawGeometry(null, new Pen(new SolidColorBrush(Colors.White), 2) { DashStyle = DashStyles.Dot }, geometry);
            }
        }
    }
}
