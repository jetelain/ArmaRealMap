using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;

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

        public override IModelDefinition<TModelInfo> SelectObjectToInsert(TerrainPoint point)
        {
            return definition.Models.GetRandom(area.Random);
        }
    }
}
