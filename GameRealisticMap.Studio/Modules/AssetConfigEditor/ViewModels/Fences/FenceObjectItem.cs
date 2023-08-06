using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets.Fences;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Fences
{
    internal class FenceObjectItem : PropertyChangedBase, IWithEditableProbability, IWithCompositionRadius
    {
        public FenceObjectItem(ItemDefinition d)
        {
            _radius = d.Radius;
            Composition = new CompositionViewModel(d.Model);
            MaxZ = d.MaxZ;
            MinZ = d.MinZ;
            _probability = d.Probability;
            MaxScale = d.MaxScale;
            MinScale = d.MinScale;
        }

        public float _radius;
        public float Radius { get { return _radius; } set { _radius = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(FitRadius)); } }

        public CompositionViewModel Composition { get; }

        public float? MaxZ { get; set; }

        public float? MinZ { get; set; }

        private double _probability;
        public double Probability
        {
            get { return _probability; }
            set { _probability = value; NotifyOfPropertyChange(); }
        }

        public float? MaxScale { get; set; }

        public float? MinScale { get; set; }
        public float FitRadius { get => Radius; set => Radius = value; }

        public ItemDefinition ToDefinition()
        {
            return new ItemDefinition(Radius, Composition.ToDefinition(), MaxZ, MinZ, Probability, MaxScale, MinScale);
        }
        public void CompositionWasRotated(int degrees)
        {
            // Nothing to do
        }
    }
}
