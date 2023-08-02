using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandDefinition]
    public class ExportLogsCommandDefinition : CommandDefinition
    {
        public const string CommandName = "ExportLogs";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return GameRealisticMap.Studio.Labels.GenerateAnErrorReport; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }
    }
}
