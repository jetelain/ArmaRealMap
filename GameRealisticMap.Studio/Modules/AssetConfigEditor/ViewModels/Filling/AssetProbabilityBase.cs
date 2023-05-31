using System;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling
{
    internal abstract class AssetProbabilityBase<TId, TDefinition> : AssetBase<TId, TDefinition>, IWithEditableProbability
        where TId : struct, Enum
        where TDefinition : class, IWithProbability
    {

        protected AssetProbabilityBase(TId id, TDefinition? definition, AssetConfigEditorViewModel parent)
            : base(id, parent)
        {
            _probability = definition?.Probability ?? 1;
        }

        private double _probability;
        public double Probability
        {
            get { return _probability; }
            set { _probability = value; NotifyOfPropertyChange(); }
        }

        protected string label = string.Empty;
        public string Label
        {
            get { return label; }
            set { label = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(TreeName)); }
        }

        public override string TreeName
        {
            get
            {
                if (string.IsNullOrEmpty(label))
                {
                    return PageTitle;
                }
                return PageTitle + " - " + label;
            }
        }
    }
}
