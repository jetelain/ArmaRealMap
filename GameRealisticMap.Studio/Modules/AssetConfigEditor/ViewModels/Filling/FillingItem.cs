using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingItem : PropertyChangedBase, IWithEditableProbability, IWithComposition, IWithCompositionRadius
    {
        public FillingItem(ClusterItemDefinition d)
        {
            _radius = d.Radius;
            _exclusiveRadius = d.ExclusiveRadius;
            Composition = new CompositionViewModel(d.Model);
            MaxZ = d.MaxZ;
            MinZ = d.MinZ;
            _probability = d.Probability;
            MaxScale = d.MaxScale;
            MinScale = d.MinScale;
        }

        public float _radius;
        public float Radius { get { return _radius; } set { _radius = value; NotifyOfPropertyChange(); } }

        public float _exclusiveRadius;
        public float ExclusiveRadius { get { return _exclusiveRadius; } set { _exclusiveRadius = value; NotifyOfPropertyChange(); } }

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

        public ClusterItemDefinition ToDefinition()
        {
            return new ClusterItemDefinition(Radius, ExclusiveRadius, Composition.ToDefinition(), MaxZ, MinZ, Probability, MaxScale, MinScale);
        }
    }
}
