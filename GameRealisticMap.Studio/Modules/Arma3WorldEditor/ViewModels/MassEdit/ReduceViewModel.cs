using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class ReduceViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldEditor;

        public ReduceViewModel(Arma3WorldEditorViewModel worldEditor)
        {
            this.worldEditor = worldEditor;
        }
    }
}
