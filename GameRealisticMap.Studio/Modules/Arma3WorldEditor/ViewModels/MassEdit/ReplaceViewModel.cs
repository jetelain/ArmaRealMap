using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.MassEdit
{
    internal class ReplaceViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldEditor;

        public ReplaceViewModel(Arma3WorldEditorViewModel worldEditor)
        {
            this.worldEditor = worldEditor;
        }
    }
}
