using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels.Export
{
    internal class FileExporterViewModel : WindowBase
    {
        private readonly Arma3WorldEditorViewModel worldEditor;

        public FileExporterViewModel(Arma3WorldEditorViewModel worldEditor)
        {
            this.worldEditor = worldEditor;
        }
    }
}
