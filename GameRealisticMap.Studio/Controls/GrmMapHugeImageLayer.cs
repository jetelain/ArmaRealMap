using System.Windows;
using System.Windows.Media;
using HugeImages;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Controls
{
    public sealed class GrmMapHugeImageLayer : FrameworkElement, IGrmMapLayer
    {
        private HugeImageSource<Rgb24>? source;

        public HugeImage<Rgb24>? Image
        {
            get { return (HugeImage<Rgb24>?)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register(nameof(Image), typeof(HugeImage<Rgb24>), typeof(GrmMapHugeImageLayer), new PropertyMetadata(null, SomePropertyChanged));

        public double Resolution
        {
            get { return (double)GetValue(ResolutionProperty); }
            set { SetValue(ResolutionProperty, value); }
        }

        public static readonly DependencyProperty ResolutionProperty =
            DependencyProperty.Register(nameof(Resolution), typeof(double), typeof(GrmMapHugeImageLayer), new PropertyMetadata(1d, SomePropertyChanged));

        private static void SomePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GrmMapHugeImageLayer)?.InvalidateVisual();
        }

        public GrmMap? ParentMap { get; set; }

        protected override void OnRender(DrawingContext dc)
        {
            var parentMap = ParentMap;
            var image = Image;
            if (image != null && parentMap != null)
            {
                if (source == null || source.HugeImage != image)
                {
                    source = new HugeImageSource<Rgb24>(image, InvalidateVisual);
                }
                var deltaX = parentMap.DeltaX;
                var deltaY = parentMap.DeltaY;
                var scale = parentMap.Scale;
                var resolution = Resolution;
                var renderSize = RenderSize;
                var actualClip = new Rect(deltaX / scale * resolution, deltaY / scale * resolution, renderSize.Width / scale * resolution, renderSize.Height / scale * resolution);
                dc.PushTransform(parentMap.Translate);
                dc.PushTransform(parentMap.ScaleTr);
                if (resolution != 1)
                {
                    dc.PushTransform(new ScaleTransform(1 / resolution, 1 / resolution));
                }
                source.DrawTo(dc, actualClip);
                if (resolution != 1)
                {
                    dc.Pop();
                }
                dc.Pop();
                dc.Pop();
            }
        }

        public void OnViewportChanged()
        {
            InvalidateVisual();
        }
    }
}
