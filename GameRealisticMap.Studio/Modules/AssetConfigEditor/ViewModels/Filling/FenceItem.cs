using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FenceItem : IWithComposition
    {
        public FenceItem(StraightSegmentDefinition d)
        {
            Size = d.Size;
            Composition = new CompositionViewModel(d.Model);
        }

        public float Size { get; set; }

        public CompositionViewModel Composition { get; set; }

        internal StraightSegmentDefinition ToDefinition()
        {
            return new StraightSegmentDefinition(Composition.ToDefinition(), Size);
        }
    }
}