using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using Gemini.Framework;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.CompositionTool
{
    /// <summary>
    /// Dockable composition-editor tool pane. Holds a reference to the currently
    /// active <see cref="IWithComposition"/> view-model (e.g. a building or cluster
    /// definition editor) and forwards undo/redo operations to its manager.
    /// </summary>
    internal interface ICompositionTool : ITool
    {
        IWithComposition? Current { get; set; }
        IUndoRedoManager? UndoRedoManager { get; set; }
    }
}
