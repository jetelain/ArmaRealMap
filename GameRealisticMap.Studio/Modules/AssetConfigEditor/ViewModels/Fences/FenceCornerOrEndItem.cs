using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FenceCornerOrEndItem : PropertyChangedBase, IWithComposition, IWithCompositionRectangle, IWithEditableProbability
    {
        public FenceCornerOrEndItem(FenceCornerOrEndDefinition d)
        {
            _probability = d.Probability;
            Composition = new CompositionViewModel(d.Model);
        }

        private double _probability;
        public double Probability
        {
            get { return _probability; }
            set { _probability = value; NotifyOfPropertyChange(); }
        }

        public CompositionViewModel Composition { get; protected set; }

        public virtual float Depth { get => 200; set { } }

        public virtual float Width { get => 200; set { } }

        internal FenceCornerOrEndDefinition ToDefinition()
        {
            return new FenceCornerOrEndDefinition(Composition.ToDefinition(), Probability);
        }

        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}
