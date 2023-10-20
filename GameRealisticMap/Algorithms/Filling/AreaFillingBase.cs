using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Algorithms.Filling
{
    internal abstract class AreaFillingBase<TModelInfo>
    {
        protected readonly AreaDefinition area;

        protected AreaFillingBase(AreaDefinition area, IWithDensity densityDefinition)
        {
            this.area = area;
            Density = densityDefinition.GetDensity(area.Random);
            ItemsToAdd = (int)Math.Ceiling(area.Polygon.Area * Density);
        }

        public double Density { get; }

        public abstract IClusterItemDefinition<TModelInfo>? SelectObjectToInsert(TerrainPoint point, IPointConditionContext conditionContext);

        public AreaDefinition Area => area;

        public int ItemsToAdd { get; }

        public virtual void Cleanup()
        {

        }
    }
}