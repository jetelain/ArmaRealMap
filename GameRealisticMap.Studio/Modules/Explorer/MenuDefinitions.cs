using System.ComponentModel.Composition;
using GameRealisticMap.Studio.Modules.Explorer.Commands;
using Gemini.Framework.Menus;

namespace GameRealisticMap.Studio.Modules.Explorer
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ViewExplorerMenuItem = new CommandMenuItemDefinition<ViewExplorerCommandDefinition>(
            Gemini.Modules.MainMenu.MenuDefinitions.ViewToolsMenuGroup, 0);
    }
}
