using System.Runtime.InteropServices;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal static class SystemNative
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);


        public static double GetTotalMemoryInGigaBytes()
        {
            if (GetPhysicallyInstalledSystemMemory(out var total))
            {
                return total / 1024 / 1024.0;
            }
            return 0;
        }
    }
}
