using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Toolkit
{
    public static class ShellHelper
    {
        public static void OpenUri(string uri)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = uri });
        }

        public static void OpenUri(Uri uri)
        {
            OpenUri(uri.OriginalString);
        }
    }
}
