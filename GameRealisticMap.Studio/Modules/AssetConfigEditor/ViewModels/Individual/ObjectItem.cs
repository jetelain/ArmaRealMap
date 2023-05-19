using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Individual
{
    internal class ObjectItem : PropertyChangedBase, IWithEditableProbability, IWithComposition
    {
        public ObjectItem(ObjectDefinition d)
        {
            Probability = d.Probability;
            Composition = new CompositionViewModel(d.Composition);
        }

        public double Probability { get; set; }

        public CompositionViewModel Composition { get; set; }

        internal ObjectDefinition ToDefinition()
        {
            return new ObjectDefinition(Composition.ToDefinition(), Probability);
        }
    }
}