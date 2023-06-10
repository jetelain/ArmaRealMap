using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Gemini.Framework.Services;
using Gemini.Modules.Shell.Views;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    [Export(typeof(IShell))]
    public class ShellViewModel : Gemini.Modules.Shell.ViewModels.ShellViewModel
    {
        static ShellViewModel()
        {
            ViewLocator.AddNamespaceMapping(typeof(ShellViewModel).Namespace, typeof(ShellView).Namespace);
        }

        public override string StateFile => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap",
            "ApplicationState.bin");

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"GameRealisticMap"));
            return base.OnDeactivateAsync(close, cancellationToken);
        }
    }

}
