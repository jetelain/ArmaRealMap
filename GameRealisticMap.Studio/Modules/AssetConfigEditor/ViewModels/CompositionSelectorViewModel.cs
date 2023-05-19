using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class CompositionSelectorViewModel : WindowBase
    {
        private readonly IModelImporterTarget target;

        public CompositionSelectorViewModel(IModelImporterTarget target)
        {
            this.target = target;
        }
    }
}