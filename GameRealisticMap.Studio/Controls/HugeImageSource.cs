using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HugeImages;
using HugeImages.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Studio.Controls
{
    internal sealed class HugeImageSource<TPixel> : IDisposable
        where TPixel : unmanaged, IPixel<TPixel>
    {
        private const int SampleSize = 512;
        private const int MaxLoaded = 256;

        private readonly HugeImage<TPixel> hugeImage;
        private readonly List<DrawPart> parts = new List<DrawPart>();
        private readonly Action invalidate;

        private sealed class DrawPart
        {
            private readonly HugeImageSource<TPixel> parent;
            private readonly SixLabors.ImageSharp.Point point;
            private BitmapSource? source;
            private byte[]? bytes;
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
                    bytes = buffer;
                    parent.invalidate();
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
                    if (bytes != null)
                    {
                        source = BitmapSource.Create((int)Rectangle.Width, (int)Rectangle.Height, 300, 300, PixelFormats.Bgra32, null, bytes, (int)Rectangle.Width * 4);
                        bytes = null;
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

            for (var x = 0; x < hugeImage.Size.Width; x += SampleSize)
            {
                for (var y = 0; y < hugeImage.Size.Height; y += SampleSize)
                {
                    parts.Add(new DrawPart(this, x, y));
                }
            }
        }

        public HugeImage<TPixel> HugeImage => hugeImage;

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
