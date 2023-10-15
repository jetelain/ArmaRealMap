using GameRealisticMap.Geometries;
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
    }
}