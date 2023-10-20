using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using MathNet.Numerics;
using WeatherStats.Stats;

namespace GameRealisticMap.Conditions
{
    internal sealed class PolygonConditionContext : IPolygonConditionContext
    {
        private readonly ConditionEvaluator conditionEvaluator;
        private readonly TerrainPolygon polygon;
        private MinMaxAvg? elevation;

        public PolygonConditionContext(ConditionEvaluator conditionEvaluator, TerrainPolygon polygon)
        {
            this.conditionEvaluator = conditionEvaluator;
            this.polygon = polygon;
        }

        public float Area => (float)polygon.Area;

        private MinMaxAvg GetElevation() => elevation = elevation ?? conditionEvaluator.GetElevation(polygon);

        public float MinElevation => GetElevation().Min;

        public float MaxElevation => GetElevation().Max;
        public float AvgElevation => GetElevation().Avg;

        public bool IsResidential => conditionEvaluator.IsArea(polygon, BuildingTypeId.Residential);

        public bool IsCommercial => conditionEvaluator.IsArea(polygon, BuildingTypeId.Commercial);

        public bool IsIndustrial => conditionEvaluator.IsArea(polygon, BuildingTypeId.Industrial);

        public bool IsRetail => conditionEvaluator.IsArea(polygon, BuildingTypeId.Retail);

        public bool IsMilitary => conditionEvaluator.IsArea(polygon, BuildingTypeId.Military);

        public bool IsFarmyard => conditionEvaluator.IsArea(polygon, BuildingTypeId.Agricultural);

    }
}