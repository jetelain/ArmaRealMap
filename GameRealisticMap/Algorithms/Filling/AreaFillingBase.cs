using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;

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

        public abstract IClusterItemDefinition<TModelInfo> SelectObjectToInsert(TerrainPoint point);

        public AreaDefinition Area => area;

        public int ItemsToAdd { get; }
    }
}