using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    [Export]
    internal class AboutViewModel : Document
    {
        public AboutViewModel() {

            using var stream = typeof(AboutViewModel).Assembly.GetManifestResourceStream("GameRealisticMap.Studio.ThirdParty.txt")!;
            ThirdParty = new StreamReader(stream).ReadToEnd();
            Version = "Version " + App.GetAppVersion();
            DisplayName = Labels.About;
        }

        public string ThirdParty { get; }
        public string Version { get; }
    }
}
