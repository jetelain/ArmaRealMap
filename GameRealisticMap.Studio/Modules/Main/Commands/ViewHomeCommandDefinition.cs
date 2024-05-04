using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.Main.Commands
{
    [CommandDefinition]
    public class ViewHomeCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.Home";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Labels.Home; }
        }

        public override string ToolTip
        {
            get { return ""; }
        }

        public override Uri IconSource => new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Tools/Home.png");
    }
}
