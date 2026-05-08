using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    /// <summary>
    /// Extension methods for Gemini's <c>IUndoRedoManager</c>.
    /// </summary>
    public static class UndoRedoManagerExtensions
    {

        /// <summary>
        /// Clears all recorded undo and redo history by temporarily setting the undo count limit to zero.
        /// </summary>
        public static void Clear(this IUndoRedoManager undoRedoManager)
        {
            undoRedoManager.UndoCountLimit = 0;
            undoRedoManager.UndoCountLimit = null;
        }
    }
}
