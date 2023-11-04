using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HugeImages;
using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Controls
{
    internal sealed class HugeImageSource<TPixel> : DispatcherObject, IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private const int SampleSize = 512;
        private const int SampleStep = SampleSize - 4;
        private const int MaxLoaded = 512; // Limit to 512 MB of memory

        private readonly HugeImage<TPixel> hugeImage;
        private readonly List<DrawPart> parts = new List<DrawPart>();
        private readonly Action invalidate;

        private sealed class DrawPart
        {
            private readonly HugeImageSource<TPixel> parent;
            private readonly SixLabors.ImageSharp.Point point;
            private BitmapSource? source;
            private int state;

            public DrawPart(HugeImageSource<TPixel> parent, int x, int y)
            {
                this.parent = parent;
                this.point = new(x, y);
                Rectangle = new Rect(x, y, Math.Min(SampleSize, parent.hugeImage.Size.Width - x), Math.Min(SampleSize, parent.hugeImage.Size.Height - y));
            }

            public Rect Rectangle { get; }

            public long LastRead { get; private set; }

            private async Task LoadImageBytes()
            {
                using (var image = new Image<Bgra32>((int)Rectangle.Width, (int)Rectangle.Height))
                {
                    await image.MutateAsync(async i => await i.DrawHugeImageAsync(parent.hugeImage, point, 1).ConfigureAwait(false)).ConfigureAwait(false);
                    var buffer = new byte[image.Width * image.Height * 4];
                    image.CopyPixelDataTo(new Span<byte>(buffer));
                    _ = parent.Dispatcher.BeginInvoke(() =>
                    {
                        source = BitmapSource.Create((int)Rectangle.Width, (int)Rectangle.Height, 300, 300, PixelFormats.Bgra32, null, buffer, (int)Rectangle.Width * 4);
                        parent.invalidate();
                    });
                }
            }

            public BitmapSource? GetSource()
            {
                if (source == null)
                {
                    if (Interlocked.Exchange(ref state, 1) == 0)
                    { 
                        Task.Run(LoadImageBytes);
                    }
                }
                LastRead = Stopwatch.GetTimestamp();
                return source;
            }

            public void Free()
            {
                if (source != null)
                {
                    state = 0;
                    source = null;
                }
            }

            public bool IsLoaded => source != null;
        }

        public HugeImageSource(HugeImage<TPixel> hugeImage, Action invalidate)
        {
            this.hugeImage = hugeImage;
            this.invalidate = invalidate;

            for (var x = 0; x < hugeImage.Size.Width; x += SampleStep)
            {
                for (var y = 0; y < hugeImage.Size.Height; y += SampleStep)
                {
                    parts.Add(new DrawPart(this, x, y));
                }
            }
        }

        public HugeImage<TPixel> HugeImage => hugeImage;

        public Brush PlaceHolderBrush { get; set; } = new SolidColorBrush(Colors.Gray);

        public void DrawTo(DrawingContext dc, Rect currentClip)
        {
            foreach(var part in parts)
            {
                if (part.Rectangle.IntersectsWith(currentClip))
                {
                    var source = part.GetSource();
                    if (source != null)
                    {
                        dc.DrawImage(source, part.Rectangle);
                    }
                    else
                    {
                        dc.DrawRectangle(PlaceHolderBrush, null, part.Rectangle);
                    }
                }
            }
            var count = parts.Count(p => p.IsLoaded);
            if (count > MaxLoaded)
            {
                foreach(var part in parts.Where(p => p.IsLoaded).OrderBy(p =>p.LastRead).Take(count - MaxLoaded))
                {
                    part.Free();
                }
            }
        }

        public void Dispose()
        {
            hugeImage.Dispose();
        }
    }
}
