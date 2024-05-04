using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandDefinition]
    public class RecentFilesCommandDefinition : CommandListDefinition
    {
        public const string CommandName = "File.Recent";

        public override string Name
        {
            get { return CommandName; }
        }
    }
}
