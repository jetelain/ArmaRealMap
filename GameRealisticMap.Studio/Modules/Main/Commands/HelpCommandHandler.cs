using System.Threading.Tasks;
using GameRealisticMap.Studio.Toolkit;
using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandHandler]
    public class HelpCommandHandler : CommandHandlerBase<HelpCommandDefinition>
    {
        public override Task Run(Command command)
        {
            ShellHelper.OpenUri(Labels.HelpHomeLink);
            return Task.CompletedTask;
        }
    }
}
