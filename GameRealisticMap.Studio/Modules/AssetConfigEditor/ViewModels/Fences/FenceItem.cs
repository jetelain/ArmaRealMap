using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FenceItem : PropertyChangedBase, IWithComposition, IWithCompositionRectangle
    {
        public FenceItem(FenceSegmentDefinition d)
        {
            _size = d.Size;
            _proportion = d.Proportion;
            Composition = new CompositionViewModel(d.Model);
        }

        private float _size;
        public float Size
        {
            get { return _size; }
            set { _size = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Width)); NotifyOfPropertyChange(nameof(Depth)); }
        }

        private float _proportion;
        public float Proportion
        {
            get { return _proportion; }
            set { _proportion = value; NotifyOfPropertyChange(); }
        }

        public CompositionViewModel Composition { get; protected set; }

        // TODO: Fences should be oriented North-South and not West-East

        public virtual float Depth { get => 200; set { } }

        public virtual float Width { get => Size; set => Size = value; }

        internal FenceSegmentDefinition ToDefinition()
        {
            return new FenceSegmentDefinition(Composition.ToDefinition(), Size, Proportion);
        }

        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}