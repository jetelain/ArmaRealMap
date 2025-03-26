using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    public static class UndoRedoManagerExtensions
    {

        public static void Clear(this IUndoRedoManager undoRedoManager)
        {
            undoRedoManager.UndoCountLimit = 0;
            undoRedoManager.UndoCountLimit = null;
        }
    }
}
