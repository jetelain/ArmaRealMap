using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Filling
{
    internal sealed class AreaFillingBasic<TModelInfo> : AreaFillingBase<TModelInfo>
    {
        private readonly IBasicDefinition<TModelInfo> definition;

        public AreaFillingBasic(AreaDefinition fillarea, IBasicDefinition<TModelInfo> definition)
            : base(fillarea, definition)
        {
            this.definition = definition;
        }

        public override IClusterItemDefinition<TModelInfo>? SelectObjectToInsert(TerrainPoint point, IPointConditionContext conditionContext)
        {
            return definition.Models.GetRandom(area.Random, conditionContext);
        }
    }
}
