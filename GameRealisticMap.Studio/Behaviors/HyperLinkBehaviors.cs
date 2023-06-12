using System.Windows.Documents;
using GameRealisticMap.Studio.Toolkit;

namespace GameRealisticMap.Studio.Behaviors
{
    internal static class HyperLinkBehaviors
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
            ShellHelper.OpenUri(e.Uri);
        }
    }
}
