using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandDefinition]
    public class HelpCommandDefinition : CommandDefinition
    {
        public const string CommandName = "Help";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Labels.Help; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }
    }
}
