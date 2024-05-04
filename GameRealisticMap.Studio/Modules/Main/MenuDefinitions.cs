using System.ComponentModel.Composition;
using GameRealisticMap.Studio.Modules.Explorer.Commands;
using GameRealisticMap.Studio.Modules.Main.Commands;
using Gemini.Framework.Menus;

namespace GameRealisticMap.Studio.Main.Explorer
{
    public static class MenuDefinitions
    {
        [Export]
        public static readonly MenuDefinition HelpMenu = new MenuDefinition(Gemini.Modules.MainMenu.MenuDefinitions.MainMenuBar, 100, Labels.Help);

        [Export]
        public static readonly MenuItemGroupDefinition HelpMenuGroup = new MenuItemGroupDefinition(HelpMenu, 0);

        [Export]
        public static readonly MenuItemDefinition ViewHomeMenuItem = new CommandMenuItemDefinition<ViewHomeCommandDefinition>(
            Gemini.Modules.MainMenu.MenuDefinitions.ToolsOptionsMenuGroup, 0);

        [Export]
        public static readonly MenuItemDefinition HelpMenuItem = new CommandMenuItemDefinition<HelpCommandDefinition>(HelpMenuGroup, 0);

        [Export]
        public static readonly MenuItemDefinition ExportLogsItem = new CommandMenuItemDefinition<ExportLogsCommandDefinition>(HelpMenuGroup, 1);

        [Export]
        public static readonly MenuItemDefinition AboutMenuItem = new CommandMenuItemDefinition<AboutCommandDefinition>(HelpMenuGroup, 2);

        [Export]
        public static readonly MenuItemGroupDefinition FileRecentMenuGroup = new MenuItemGroupDefinition(Gemini.Modules.MainMenu.MenuDefinitions.FileMenu, 9);

        [Export]
        public static readonly MenuItemDefinition FileRecentItem = new TextMenuItemDefinition(
            FileRecentMenuGroup, 0, Labels.RecentFiles);

        [Export]
        public static readonly MenuItemGroupDefinition FileRecentCascadeGroup = new MenuItemGroupDefinition(
            FileRecentItem, 0);

        [Export]
        public static readonly MenuItemDefinition FileRecentMenuItemList = new CommandMenuItemDefinition<RecentFilesCommandDefinition>(
            FileRecentCascadeGroup, 0);

    }
}
