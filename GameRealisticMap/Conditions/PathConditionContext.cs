using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using WeatherStats.Stats;

namespace GameRealisticMap.Conditions
{
    internal sealed class PathConditionContext : IPathConditionContext
    {
        private readonly ConditionEvaluator conditionEvaluator;
        private readonly TerrainPath path;
        private MinMaxAvg? elevation;

        public PathConditionContext(ConditionEvaluator conditionEvaluator, TerrainPath path)
        {
            this.conditionEvaluator = conditionEvaluator;
            this.path = path;
        }

        public float Length => path.Length;

        private MinMaxAvg GetElevation() => elevation = elevation ?? conditionEvaluator.GetElevation(path);

        public float MinElevation => GetElevation().Min;

        public float MaxElevation => GetElevation().Max;

        public float AvgElevation => GetElevation().Avg; 
        
        public bool IsResidential => conditionEvaluator.IsArea(path, BuildingTypeId.Residential);

        public bool IsCommercial => conditionEvaluator.IsArea(path, BuildingTypeId.Commercial);

        public bool IsIndustrial => conditionEvaluator.IsArea(path, BuildingTypeId.Industrial);

        public bool IsRetail => conditionEvaluator.IsArea(path, BuildingTypeId.Retail);

        public bool IsMilitary => conditionEvaluator.IsArea(path, BuildingTypeId.Military);

        public bool IsFarmyard => conditionEvaluator.IsArea(path, BuildingTypeId.Agricultural);
    }
}