using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Algorithms.Filling
{
    internal class ClusterCollectionDefinitionMock : IClusterCollectionDefinition<string>, IEnumerable<ClusterDefinitionMock>, IWithDensity
    {
        private readonly List<ClusterDefinitionMock> clusters = new List<ClusterDefinitionMock>();

        public ClusterCollectionDefinitionMock(double density)
        {
            MinDensity = density;
            MaxDensity = density;
        }

        public IReadOnlyList<IClusterDefinition<string>> Clusters => clusters;

        public double MinDensity { get; set; }

        public double MaxDensity { get; set; }

        public ICondition<IPolygonConditionContext>? Condition { get; set; }

        public double Probability { get; set; } = 1;

        public IWithDensity Default => this;

        public IDensityWithNoiseDefinition? LargeAreas => null;

        public IEnumerator<ClusterDefinitionMock> GetEnumerator()
        {
            return clusters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return clusters.GetEnumerator();
        }

        public void Add(ClusterDefinitionMock item)
        {
            clusters.Add(item);
        }
    }
}
