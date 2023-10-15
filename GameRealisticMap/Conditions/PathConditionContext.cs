using GameRealisticMap.Geometries;
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
    }
}