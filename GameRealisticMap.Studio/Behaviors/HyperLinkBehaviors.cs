using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace GameRealisticMap.Studio.Behaviors
{
    internal class HyperLinkBehaviors
    {
        public static void SetShellExecute(Hyperlink hyperlink, bool value)
        {
            if ( value)
            {
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            }
        }

        private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { UseShellExecute = true, FileName = e.Uri.OriginalString });
        }
    }
}
