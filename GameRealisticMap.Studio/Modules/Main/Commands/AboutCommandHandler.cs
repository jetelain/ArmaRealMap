using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Studio.Modules.Main.ViewModels;
using Gemini.Framework.Commands;
using Gemini.Framework.Services;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandHandler]
    public class AboutCommandHandler : CommandHandlerBase<AboutCommandDefinition>
    {
        private readonly IShell _shell;

        [ImportingConstructor]
        public AboutCommandHandler(IShell shell)
        {
            _shell = shell;
        }

        public override Task Run(Command command)
        {
            return _shell.OpenDocumentAsync(IoC.Get<AboutViewModel>());
        }
    }
}
