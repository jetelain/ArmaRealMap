using System.ComponentModel.Composition;
using GameRealisticMap.Studio.Modules.AssetBrowser.Commands;
using Gemini.Framework.Menus;

namespace GameRealisticMap.Studio.AssetBrowser.Explorer
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition ToolsArma3MenuItem = new TextMenuItemDefinition(
            Gemini.Modules.MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 0, "Arma 3");

        [Export]
        public static readonly MenuItemGroupDefinition ToolsArma3MenuGroup = new MenuItemGroupDefinition(
            ToolsArma3MenuItem, 0);

        [Export]
        public static readonly MenuItemDefinition AssetMenuItem = new CommandMenuItemDefinition<ViewAssetsBrowserCommandDefinition>(
            ToolsArma3MenuGroup, 0);

        [Export]
        public static readonly MenuItemDefinition GdtMenuItem = new CommandMenuItemDefinition<ViewGdtBrowserCommandDefinition>(
            ToolsArma3MenuGroup, 1);
    }
}
