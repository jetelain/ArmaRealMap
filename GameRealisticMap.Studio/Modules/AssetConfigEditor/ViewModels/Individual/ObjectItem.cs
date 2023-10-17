using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using GameRealisticMap.Studio.Modules.ConditionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class ObjectItem : PropertyChangedBase, IWithEditableProbability, IWithComposition
    {
        public ObjectItem(ObjectDefinition d)
        {
            _probability = d.Probability;
            Condition = new PointConditionVM(d.Condition);
            Composition = new CompositionViewModel(d.Composition);
        }

        private double _probability;
        public double Probability
        {
            get { return _probability; }
            set { _probability = value; NotifyOfPropertyChange(); }
        }

        public PointConditionVM Condition { get; }

        public CompositionViewModel Composition { get; }

        internal ObjectDefinition ToDefinition()
        {
            return new ObjectDefinition(Composition.ToDefinition(), Probability, Condition.ToDefinition());
        }

        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}