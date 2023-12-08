using System.Collections;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    internal class ClusterDefinitionMock : IClusterDefinition<string>, IEnumerable<ClusterItemDefinitionMock>
    {
        private readonly List<ClusterItemDefinitionMock> models = new List<ClusterItemDefinitionMock>();

        public ClusterDefinitionMock(double probability)
        {
            Probability = probability;
        }

        public IReadOnlyList<IClusterItemDefinition<string>> Models => models;

        public ICondition<IPointConditionContext>? Condition { get; set; }

        public double Probability { get; }

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