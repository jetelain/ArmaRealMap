using System.Linq;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Filling;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal class FillingItem : PropertyChangedBase, IWithEditableProbability, IWithComposition
    {
        public FillingItem(ClusterItemDefinition d)
        {
            Radius = d.Radius;
            ExclusiveRadius = d.ExclusiveRadius;
            Composition = new CompositionViewModel(d.Model);
            MaxZ = d.MaxZ;
            MinZ = d.MinZ;
            Probability = d.Probability;
            MaxScale = d.MaxScale;
            MinScale = d.MinScale;
        }

        public float Radius { get; set; }

        public float ExclusiveRadius { get; set; }

        public CompositionViewModel Composition { get; set; }

        public float? MaxZ { get; set; }

        public float? MinZ { get; set; }

        public double Probability { get; set; }

        public float? MaxScale { get; set; }

        public float? MinScale { get; set; }

        public ClusterItemDefinition ToDefinition()
        {
            return new ClusterItemDefinition(Radius, ExclusiveRadius, Composition.ToDefinition(), MaxZ, MinZ, Probability, MaxScale, MinScale);
        }
    }
}
