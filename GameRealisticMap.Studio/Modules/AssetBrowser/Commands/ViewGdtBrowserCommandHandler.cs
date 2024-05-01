using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.AssetBrowser.ViewModels;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;
using GameRealisticMap.Studio.Modules.Main.ViewModels;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;
using Gemini.Framework.Threading;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Commands
{
    [CommandHandler]
    public class ViewGdtBrowserCommandHandler : CommandHandlerBase<ViewGdtBrowserCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public ViewGdtBrowserCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.OpenDocumentAsync(IoC.Get<GdtBrowserViewModel>());
        }
    }
}
