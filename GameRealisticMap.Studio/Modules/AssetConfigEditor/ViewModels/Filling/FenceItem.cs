using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FenceItem : PropertyChangedBase, IWithComposition, IWithCompositionRectangle
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
            set { _size = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Width)); NotifyOfPropertyChange(nameof(Depth)); }
        }

        public CompositionViewModel Composition { get; protected set; }

        // TODO: Fences should be oriented North-South and not West-East

        public virtual float Depth { get => 200; set { } }

        public virtual float Width { get => Size; set => Size = value; }

        internal StraightSegmentDefinition ToDefinition()
        {
            return new StraightSegmentDefinition(Composition.ToDefinition(), Size);
        }
    }
}