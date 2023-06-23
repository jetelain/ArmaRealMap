using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gemini.Framework.Commands;

namespace GameRealisticMap.Studio.Modules.AssetBrowser.Commands
{
    [CommandDefinition]
    public class ViewAssetsBrowserCommandDefinition : CommandDefinition
    {
        public const string CommandName = "View.AssetsBrowser";

        public override string Name
        {
            get { return CommandName; }
        }

        public override string Text
        {
            get { return Labels.AssetsBrowser; }
        }

        public override Uri IconSource => new Uri("pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/Objects.png");

        public override string ToolTip
        {
            get { return ""; }
        }
    }
}
