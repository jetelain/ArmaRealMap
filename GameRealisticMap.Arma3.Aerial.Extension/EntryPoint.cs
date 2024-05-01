using System.Runtime.InteropServices;
using System.Text;

namespace GameRealisticMap.Arma3.Aerial.Extension
{
    internal static class EntryPoint
    {
        private static AerialPhotoWorker? instance;

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionRegisterCallback")]
        public static void RVExtensionRegisterCallback(nint func)
        {

        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionVersion")]
        public static void RvExtensionVersion(nint output, int outputSize)
        {
            Output(output, outputSize, "GRM Aerial 1.0");
        }

        private static void Output(nint output, int outputSize, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            Marshal.Copy(bytes, 0, output, Math.Min(bytes.Length, outputSize));
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtension")]
        public static void RvExtension(nint output, int outputSize, nint function)
        {
            var functionString = Marshal.PtrToStringUTF8(function);
            DoWork(functionString, new string[0]);
            Output(output, outputSize, "");
        }

        [UnmanagedCallersOnly(EntryPoint = "RVExtensionArgs")]
        public static int RvExtensionArgs(nint output, int outputSize, nint function, nint args, int argCount)
        {
            var functionString = Marshal.PtrToStringUTF8(function);
            var argsString = new string?[argCount];
            for (int i = 0; i < argCount; i++)
            {
                argsString[i] = Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(args + (i * Marshal.SizeOf<nint>())));
            }

            DoWork(functionString, argsString);
            Output(output, outputSize, "");
            return 0;
        }

        private static void DoWork(string? functionString, string?[] argsString)
        {
            try
            {
                if ( instance == null )
                {
                    instance = new AerialPhotoWorker();
                }
                switch(functionString)
                {
                    case "TakeClear":
                        instance.TakeClear();
                        break;
                    case "TakeImage":
                        instance.TakeImage(argsString!);
                        break;
                }
            }
            catch(Exception e)
            {
                instance?.AppendToLog(e.ToString());
            }
        }
    }
}

