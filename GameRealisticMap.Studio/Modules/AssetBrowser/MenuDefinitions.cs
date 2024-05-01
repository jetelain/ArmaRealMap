using System.ComponentModel.Composition;
using GameRealisticMap.Studio.Modules.AssetBrowser.Commands;
using GameRealisticMap.Studio.Modules.Explorer.Commands;
using GameRealisticMap.Studio.Modules.Main.Commands;
using Gemini.Framework.Menus;

namespace GameRealisticMap.Studio.AssetBrowser.Explorer
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuItemDefinition AssetMenuItem = new CommandMenuItemDefinition<ViewAssetsBrowserCommandDefinition>(
            Gemini.Modules.MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 0);

        [Export]
        public static readonly MenuItemDefinition GdtMenuItem = new CommandMenuItemDefinition<ViewGdtBrowserCommandDefinition>(
            Gemini.Modules.MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 1);
    }
}
