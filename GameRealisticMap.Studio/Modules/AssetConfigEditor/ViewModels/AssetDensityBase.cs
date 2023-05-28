using System;
using System.Threading.Tasks;
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
            _minDensity = definition?.MinDensity ?? 0.01;
            _maxDensity = definition?.MaxDensity ?? 0.01;
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

        public double? ComputedMaxDensity { get; set; }

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

        protected abstract double GetMaxDensity();

        public Task ComputeMaxDensity()
        {
            var max = Math.Round(GetMaxDensity(), 4);
            ComputedMaxDensity = max;
            if (_maxDensity > max)
            {
                _maxDensity = max;
            }
            if (_minDensity > _maxDensity)
            {
                _minDensity = _maxDensity;
            }
            NotifyOfPropertyChange(nameof(ComputedMaxDensity));
            NotifyOfPropertyChange(nameof(MaxDensity));
            NotifyOfPropertyChange(nameof(MinDensity));
            NotifyOfPropertyChange(nameof(DensityText));
            return Task.CompletedTask;
        }

    }
}
