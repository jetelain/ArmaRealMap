using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameRealisticMap.Arma3.Aerial
{
    internal sealed class ScreenshotWorker : IDisposable
    {
        private readonly Bitmap bitmap;
        private readonly Graphics graphics;
        private readonly nint hwnd;

        public ScreenshotWorker(nint hwnd, int pxWidth = 1024, int pxHeight = 768)
        {
            this.hwnd = hwnd;
            bitmap = new Bitmap(pxWidth, pxHeight, PixelFormat.Format32bppArgb);
            graphics = Graphics.FromImage(bitmap);
        }

        public static ScreenshotWorker FromProcess(Process process, int pxWidth = 1024, int pxHeight = 768)
        {
            process.Refresh();
            return new ScreenshotWorker(process.MainWindowHandle, pxWidth, pxHeight);
        }

        public void CaptureTo(string fileName)
        {
            var point = new Native.Point();
            Native.ClientToScreen(hwnd, ref point);
            graphics.CopyFromScreen(point.x, point.y, 0, 0, bitmap.Size, CopyPixelOperation.SourceCopy);
            bitmap.Save(fileName);
        }

        public void Dispose()
        {
            graphics.Dispose();
            bitmap.Dispose();
        }
    }
}
