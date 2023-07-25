using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class RailwayCrossingItem : PropertyChangedBase, IWithComposition, IWithCompositionRectangle
    {
        public RailwayCrossingItem(RailwayCrossingDefinition d)
        {
            _size = d.Size;
            _roadMaxSize = d.RoadMaxSize;
            Composition = new CompositionViewModel(d.Model);
        }

        private float _size;
        public float Size 
        { 
            get { return _size; }
            set { _size = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(Depth)); }
        }

        private float _roadMaxSize;
        public float RoadMaxSize
        {
            get { return _roadMaxSize; }
            set { _roadMaxSize = value; NotifyOfPropertyChange(); }
        }

        public CompositionViewModel Composition { get; protected set; }

        public virtual float Depth { get => Size; set => Size = value; }

        public virtual float Width { get => 200; set { } }

        internal RailwayCrossingDefinition ToDefinition()
        {
            return new RailwayCrossingDefinition(Composition.ToDefinition(), Size, RoadMaxSize);
        }

        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}