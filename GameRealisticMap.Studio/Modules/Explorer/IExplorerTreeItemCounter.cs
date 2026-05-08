namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    /// <summary>
    /// Extends <see cref="IExplorerTreeItem"/> with child-item count display.
    /// Nodes implementing this interface show a badge with the number of children
    /// (e.g. "Roads (42)") in the Explorer tree.
    /// </summary>
    public interface IExplorerTreeItemCounter : IExplorerTreeItem
    {

    }
}