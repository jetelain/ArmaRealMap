using System;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal abstract class AssetDensityBase<TId, TDefinition> : AssetProbabilityBase<TId,TDefinition>, IWithEditableProbability
        where TId : struct, Enum 
        where TDefinition : class, IWithDensity, IWithProbability
    {

        protected AssetDensityBase(TId id, TDefinition? definition, AssetConfigEditorViewModel parent) 
            : base(id, definition, parent)
        {
            _minDensity = definition?.MinDensity ?? 1;
            _maxDensity = definition?.MaxDensity ?? 1;
        }

        private double _minDensity;
        public double MinDensity
        {
            get { return _minDensity; }
            set { _minDensity = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(DensityText)); }
        }

        private double _maxDensity;
        public double MaxDensity
        {
            get { return _maxDensity; }
            set { _maxDensity = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(nameof(DensityText)); }
        }

        public string DensityText
        {
            get
            {
                if (MaxDensity == MinDensity)
                {
                    return $"{MaxDensity} objects/m²";
                }
                return $"{MinDensity} to {MaxDensity} objects/m²";
            }
        }
    }
}
