using System;
using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal abstract class AssetProbabilityBase<TId, TDefinition> : AssetBase<TId,TDefinition>, IWithEditableProbability
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

    }
}
