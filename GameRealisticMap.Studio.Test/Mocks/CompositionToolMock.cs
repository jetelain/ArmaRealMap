using GameRealisticMap.Studio.Modules.CompositionTool;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using Gemini.Framework;
using Gemini.Framework.Services;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Test.Mocks
{
    internal class CompositionToolMock : Tool, ICompositionTool
    {
        public IWithComposition? Current { get; set; }

        public IUndoRedoManager? UndoRedoManager { get; set; }

        public override PaneLocation PreferredLocation => throw new NotImplementedException();
    }
}