using System.Collections;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    internal class BasicDefinitionMock : IBasicDefinition<string>, IEnumerable<ClusterItemDefinitionMock>, IWithDensity
    {
        private readonly List<ClusterItemDefinitionMock> models = new List<ClusterItemDefinitionMock>();

        public BasicDefinitionMock(double density)
        {
            MinDensity = density;
            MaxDensity = density;
        }

        public IReadOnlyList<IClusterItemDefinition<string>> Models => models;

        public ICondition<IPolygonConditionContext>? Condition { get; set; }

        public double Probability { get; set; } = 1;

        public double MinDensity { get; set; }

        public double MaxDensity { get; set; }

        public IWithDensity Default => this;

        public IDensityWithNoiseDefinition? LargeAreas => null;

        public IEnumerator<ClusterItemDefinitionMock> GetEnumerator()
        {
            return models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return models.GetEnumerator();
        }

        public void Add(ClusterItemDefinitionMock item)
        {
            models.Add(item);
        }
    }
}