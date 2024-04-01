using System;
using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Commands
{
    [CommandDefinition]
    public class ViewGdtBrowserCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.GdtBrowser";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Labels.GdtBrowserTitle; }
        }

        public override Uri IconSource => new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Materials.png");

        public override string ToolTip
        {
            get { return ""; }
        }
    }
}
