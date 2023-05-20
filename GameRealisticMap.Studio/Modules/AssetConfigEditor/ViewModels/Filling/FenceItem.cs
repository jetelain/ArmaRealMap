using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FenceItem : PropertyChangedBase, IWithComposition
    {
        public FenceItem(StraightSegmentDefinition d)
        {
            _size = d.Size;
            Composition = new CompositionViewModel(d.Model);
        }

        private float _size;
        public float Size 
        { 
            get { return _size; }
            set { _size = value; NotifyOfPropertyChange(); }
        }

        public CompositionViewModel Composition { get; protected set; }

        internal StraightSegmentDefinition ToDefinition()
        {
            return new StraightSegmentDefinition(Composition.ToDefinition(), Size);
        }
    }
}