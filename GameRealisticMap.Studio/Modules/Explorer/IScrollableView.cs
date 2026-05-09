namespace GameRealisticMap.Studio.Modules.Explorer
{
    /// <summary>
    /// Implemented by the Explorer tree view to allow programmatic scrolling
    /// of a specific item into the visible viewport.
    /// </summary>
    internal interface IScrollableView
    {
        void ScrollIntoView(object? dataContext);
    }
}
