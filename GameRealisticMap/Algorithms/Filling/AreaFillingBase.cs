using GameRealisticMap.Geometries;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;
using GameRealisticMap.Algorithms.RandomGenerators;

namespace GameRealisticMap.Algorithms.Filling
{
    internal abstract class AreaFillingBase<TModelInfo>
    {
        protected readonly AreaDefinition area;
        private readonly IRandomPointGenerator randomPointGenerator;

        protected AreaFillingBase(AreaDefinition area, IDensityDefinition densityDefinition)
        {
            this.area = area;

            if (densityDefinition.LargeAreas == null || IsSmallOrNarrow(area.Polygon))
            {
                Density = densityDefinition.Default.GetDensity(area.Random);
                randomPointGenerator = new UniformRandomPointGenerator(area.Random, area.Polygon.MinPoint, area.Polygon.MaxPoint);
            }
            else
            {
                Density = densityDefinition.LargeAreas.GetDensity(area.Random);
                randomPointGenerator = RandomPointGenerator.Create(area.Random, area.Polygon, densityDefinition.LargeAreas);
            }

            ItemsToAdd = (int)Math.Ceiling(area.Polygon.Area * Density);
        }

        private static bool IsSmallOrNarrow(TerrainPolygon polygon)
        {
            // Make it configurable ?
            var area = polygon.Area;
            if (area < 1000)
            {
                return true;
            }
            var diagonal = polygon.MaxPoint.Vector - polygon.MinPoint.Vector;
            var narrowing = area / diagonal.Length();
            return narrowing < 20;
        }

        public double Density { get; }

        public abstract IClusterItemDefinition<TModelInfo>? SelectObjectToInsert(TerrainPoint point, IPointConditionContext conditionContext);

        public AreaDefinition Area => area;

        public int ItemsToAdd { get; }

        public virtual void Cleanup()
        {

        }

        internal TerrainPoint GetRandomPoint()
        {
            return randomPointGenerator.GetRandomPoint();
        }

        internal TerrainPoint? GetRandomPointInside()
        {
            var point = GetRandomPoint();
            int attempts = 0;
            while (!area.Polygon.Contains(point))
            {
                point = GetRandomPoint();
                attempts++;
                if (attempts > 200_000) // Safeguard
                {
                    return null;
                }
            }
            return point;
        }
    }
}