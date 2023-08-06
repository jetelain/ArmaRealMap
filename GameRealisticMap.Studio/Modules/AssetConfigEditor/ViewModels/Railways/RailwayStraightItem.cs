using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Railways
{
    internal class RailwayStraightItem : PropertyChangedBase, IWithComposition, IWithCompositionRectangle
    {
        public RailwayStraightItem(StraightSegmentDefinition d)
        {
            _size = d.Size;
            Composition = new CompositionViewModel(d.Model);
        }

        private float _size;
        public float Size
        {
            get { return _size; }
            set { _size = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Width)); NotifyOfPropertyChange(nameof(Depth)); }
        }

        public CompositionViewModel Composition { get; protected set; }

        public virtual float Depth { get => Size; set => Size = value; }

        public virtual float Width { get => 200; set { } }

        internal StraightSegmentDefinition ToDefinition()
        {
            return new StraightSegmentDefinition(Composition.ToDefinition(), Size);
        }

        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}