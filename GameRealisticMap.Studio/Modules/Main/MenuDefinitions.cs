using System.ComponentModel.Composition;
using GameRealisticMap.Studio.Modules.Explorer.Commands;
using GameRealisticMap.Studio.Modules.Main.Commands;
using Gemini.Framework.Menus;

namespace GameRealisticMap.Studio.Main.Explorer
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewHomeMenuItem = new CommandMenuItemDefinition<ViewHomeCommandDefinition>(
            Gemini.Modules.MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 0);
    }
}
