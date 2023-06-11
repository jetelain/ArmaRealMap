using System.Collections.Generic;
using GameRealisticMap.Studio.Modules.Explorer.ViewModels;

namespace GameRealisticMap.Studio.Modules.Explorer
{
    internal class ExplorerTreeItem : IExplorerTreeItem
    {
        public ExplorerTreeItem(string displayName, IEnumerable<IExplorerTreeItem> children, string iconName)
        {
            TreeName = displayName;
            Children = children;
            Icon = $"pack://application:,,,/GameRealisticMap.Studio;component/Resources/Icons/{iconName}.png";
        }

        public string TreeName { get; }

        public IEnumerable<IExplorerTreeItem> Children { get; }

        public string Icon { get; }
    }
}
