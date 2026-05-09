using System.Collections.Generic;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    /// <summary>
    /// Marks an <see cref="IExplorerTreeItem"/> as a top-level root node in the Explorer tree.
    /// Root items are collected by <see cref="ExplorerViewModel"/> and displayed as the
    /// first-level entries of the project structure.
    /// </summary>
    public interface IExplorerRootTreeItem : IExplorerTreeItem
    {

    }
}