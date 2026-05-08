using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Explorer.ViewModels
{
    /// <summary>
    /// Marker interface for the Explorer tool pane. MEF-exported implementations
    /// are discovered by Gemini as a dockable <c>ITool</c> panel.
    /// </summary>
    internal interface IExplorerTool : ITool
    {
    }
}