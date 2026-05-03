using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    /// <summary>
    /// Implemented by view-models that own a <see cref="CompositionViewModel"/>.
    /// Allows the composition tool pane to bind to the active document's composition
    /// and receive rotation events from the 2D canvas.
    /// </summary>
    internal interface IWithComposition
    {
        CompositionViewModel Composition { get; }

        void CompositionWasRotated(int degrees);
    }
}
