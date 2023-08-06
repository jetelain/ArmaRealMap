using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GameRealisticMap.Studio.Toolkit
{
    internal class PathUtility
    {
        // Source : https://stackoverflow.com/questions/410705/best-way-to-determine-if-two-path-reference-to-same-file-in-c-sharp

        [StructLayout(LayoutKind.Explicit)]
        private struct BY_HANDLE_FILE_INFORMATION
        {
            [FieldOffset(28)]
            public uint VolumeSerialNumber;

            [FieldOffset(44)]
            public uint FileIndexHigh;

            [FieldOffset(48)]
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
          [MarshalAs(UnmanagedType.U4)] FileAccess access,
          [MarshalAs(UnmanagedType.U4)] FileShare share,
          IntPtr securityAttributes,
          [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
          [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
          IntPtr templateFile);

        public static bool IsSameFile(string path1, string path2)
        {
            using (SafeFileHandle sfh1 = CreateFile(path1, FileAccess.Read, FileShare.ReadWrite,
                IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
            {
                if (sfh1.IsInvalid)
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                using (SafeFileHandle sfh2 = CreateFile(path2, FileAccess.Read, FileShare.ReadWrite,
                  IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero))
                {
                    if (sfh2.IsInvalid)
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

                    BY_HANDLE_FILE_INFORMATION fileInfo1;
                    bool result1 = GetFileInformationByHandle(sfh1, out fileInfo1);
                    if (!result1)
                        throw new IOException(string.Format("GetFileInformationByHandle has failed on {0}", path1));

                    BY_HANDLE_FILE_INFORMATION fileInfo2;
                    bool result2 = GetFileInformationByHandle(sfh2, out fileInfo2);
                    if (!result2)
                        throw new IOException(string.Format("GetFileInformationByHandle has failed on {0}", path2));

                    return fileInfo1.VolumeSerialNumber == fileInfo2.VolumeSerialNumber
                      && fileInfo1.FileIndexHigh == fileInfo2.FileIndexHigh
                      && fileInfo1.FileIndexLow == fileInfo2.FileIndexLow;
                }
            }
        }
    }
}
