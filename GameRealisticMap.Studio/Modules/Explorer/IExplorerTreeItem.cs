using System.Collections.Generic;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    /// <summary>
    /// Represents a single node in the Studio project Explorer tree.
    /// Implement this interface on any view-model that should appear in the
    /// left-hand Explorer panel as a navigable document or folder node.
    /// </summary>
    public interface IExplorerTreeItem
    {
        string TreeName { get; }

        string Icon { get; }

        IEnumerable<IExplorerTreeItem> Children { get; }
    }
}