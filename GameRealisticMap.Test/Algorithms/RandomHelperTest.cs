using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Test.Algorithms.Filling;

namespace GameRealisticMap.Test.Algorithms
{
    public class RandomHelperTest
    {
        private class WithProbabilityMock : IWithProbability
        {
            public double Probability { get; init; }
        }

        private class WithDensityMock : IWithDensity
        {
            public double MinDensity { get; init; }
            public double MaxDensity { get; init; }
        }

        [Fact]
        public void CheckProbabilitySum_EmptyList_DoesNotThrow()
        {
            var list = new List<WithProbabilityMock>().AsReadOnly();
            list.CheckProbabilitySum(); // no exception
        }

        [Fact]
        public void CheckProbabilitySum_SumIsOne_DoesNotThrow()
        {
            var list = new List<WithProbabilityMock>
            {
                new WithProbabilityMock { Probability = 0.4 },
                new WithProbabilityMock { Probability = 0.6 },
            }.AsReadOnly();
            list.CheckProbabilitySum(); // no exception
        }

        [Fact]
        public void CheckProbabilitySum_SumIsNotOne_Throws()
        {
            var list = new List<WithProbabilityMock>
            {
                new WithProbabilityMock { Probability = 0.3 },
                new WithProbabilityMock { Probability = 0.3 },
            }.AsReadOnly();
            Assert.Throws<ArgumentException>(() => list.CheckProbabilitySum());
        }

        [Fact]
        public void GetBetween_MinEqualsMax_ReturnsMin()
        {
            Assert.Equal(5f, RandomHelper.GetBetween(new Random(0), 5f, 5f));
        }

        [Fact]
        public void GetBetween_Range_ReturnsValueInRange()
        {
            var random = new Random(42);
            for (int i = 0; i < 100; i++)
            {
                var v = RandomHelper.GetBetween(random, 1f, 10f);
                Assert.InRange(v, 1f, 10f);
            }
        }

        [Fact]
        public void GetDensity_MinEqualsMax_ReturnsMin()
        {
            var def = new WithDensityMock { MinDensity = 0.5, MaxDensity = 0.5 };
            Assert.Equal(0.5, def.GetDensity(new Random(0)));
        }

        [Fact]
        public void GetDensity_Range_ReturnsValueInRange()
        {
            var def = new WithDensityMock { MinDensity = 0.1, MaxDensity = 0.9 };
            var random = new Random(7);
            for (int i = 0; i < 100; i++)
            {
                var v = def.GetDensity(random);
                Assert.InRange(v, 0.1, 0.9);
            }
        }

        [Fact]
        public void GetRandom_SingleItem_ReturnsThatItem()
        {
            var items = new List<ClusterItemDefinitionMock>
            {
                new ClusterItemDefinitionMock("A", 1.0, 1f),
            }.AsReadOnly();
            var result = items.GetRandom(new Random(0));
            Assert.Equal("A", result.Model);
        }

        [Fact]
        public void GetRandom_TwoItems_ReturnsItemByProbability()
        {
            var items = new List<ClusterItemDefinitionMock>
            {
                new ClusterItemDefinitionMock("A", 0.5, 1f),
                new ClusterItemDefinitionMock("B", 0.5, 1f),
            }.AsReadOnly();

            var counts = new Dictionary<string, int> { ["A"] = 0, ["B"] = 0 };
            var random = new Random(0);
            for (int i = 0; i < 1000; i++)
            {
                counts[items.GetRandom(random).Model]++;
            }

            // Both should be selected roughly half the time
            Assert.InRange(counts["A"], 400, 600);
            Assert.InRange(counts["B"], 400, 600);
        }

        [Fact]
        public void CreateRandom_FromTerrainPoint_ReturnsDeterministicRandom()
        {
            var point = new TerrainPoint(100f, 200f);
            var r1 = RandomHelper.CreateRandom(point);
            var r2 = RandomHelper.CreateRandom(point);

            Assert.Equal(r1.Next(), r2.Next());
        }

        [Fact]
        public void CreateRandom_FromString_ReturnsDeterministicRandom()
        {
            var r1 = RandomHelper.CreateRandom("test-seed");
            var r2 = RandomHelper.CreateRandom("test-seed");

            Assert.Equal(r1.Next(), r2.Next());
        }
    }
}
