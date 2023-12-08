using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    internal class ClusterItemDefinitionMock : IClusterItemDefinition<string>
    {
        public ClusterItemDefinitionMock(string model, double probability, float radius, float? fitRadius = null) 
        {
            Model = model;
            Probability = probability;
            Radius = radius;
            FitRadius = fitRadius ?? radius;
        }

        public float FitRadius { get; set; }

        public float Radius { get; set; }

        public string Model { get; }

        public float? MaxZ { get; set; }

        public float? MinZ { get; set; }

        public float? MaxScale { get; set; }

        public float? MinScale { get; set; }

        public ICondition<IPointConditionContext>? Condition { get; set; }

        public double Probability { get; set; }
    }
}