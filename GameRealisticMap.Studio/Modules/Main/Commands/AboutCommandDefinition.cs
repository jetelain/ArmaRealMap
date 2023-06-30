using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandDefinition]
    public class AboutCommandDefinition : CommandDefinition
    {
        public const string CommandName = "About";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Labels.About; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }
    }
}
