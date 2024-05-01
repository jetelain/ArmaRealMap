using System.Runtime.InteropServices;

namespace GameRealisticMap.Arma3.Aerial
{
    internal static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Point
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        internal static extern bool ClientToScreen(nint hWnd, ref Point lpPoint);
    }
}
