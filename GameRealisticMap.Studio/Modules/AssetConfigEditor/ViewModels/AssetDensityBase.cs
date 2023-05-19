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
            MinDensity = definition?.MinDensity ?? 1;
            MaxDensity = definition?.MaxDensity ?? 1;
        }

        public double MinDensity { get; set; }

        public double MaxDensity { get; set; }

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
