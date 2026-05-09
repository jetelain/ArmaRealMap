using GameRealisticMap.Algorithms;
using GameRealisticMap.Test.Algorithms.Filling;

namespace GameRealisticMap.Test.Algorithms
{
    public class DensityHelperTest
    {
        [Fact]
        public void GetMaxDensity_Items_MatchesExpectedFormula()
        {
            var items = new List<ClusterItemDefinitionMock>
            {
                new ClusterItemDefinitionMock("A", 0.5, 2f),
                new ClusterItemDefinitionMock("B", 0.5, 3f),
            };

            var expected = 0.8 / ((0.5 * Math.PI * 9) + (0.5 * Math.PI * 4));
            var actual = DensityHelper.GetMaxDensity(items.Cast<GameRealisticMap.Algorithms.Definitions.IClusterItemDefinition<string>>());

            Assert.Equal(expected, actual, 10);
        }

        [Fact]
        public void GetMaxDensity_SingleItem_MatchesExpectedFormula()
        {
            // density = 0.8 / (probability * PI * radius^2)
            var items = new List<ClusterItemDefinitionMock>
            {
                new ClusterItemDefinitionMock("A", 1.0, 1f),
            };

            var expected = 0.8 / (1.0 * Math.PI * 1.0);
            var actual = DensityHelper.GetMaxDensity(items.Cast<GameRealisticMap.Algorithms.Definitions.IClusterItemDefinition<string>>());

            Assert.Equal(expected, actual, 10);
        }
    }
}
