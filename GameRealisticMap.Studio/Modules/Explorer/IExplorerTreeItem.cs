using System.Collections.Generic;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    public interface IExplorerTreeItem
    {
        string TreeName { get; }

        string Icon { get; }

        IEnumerable<IExplorerTreeItem> Children { get; }
    }
}