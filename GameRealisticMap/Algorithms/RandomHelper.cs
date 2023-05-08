using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Algorithms
{
    public static class RandomHelper
    {
        public static void CheckProbabilitySum<T>(this IEnumerable<T> list) where T : IWithProbability
        {
            var sum = list.Sum(l => l.Probability);
            if (sum != 1)
            {
                throw new ArgumentException($"Sum of probability must be 1, but is {sum}");
            }
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list, Random random) where T : IWithProbability
        {
            if (list.Count == 1)
            {
                return list[0];
            }
            return ((IEnumerable<T>)list).GetRandom(random);
        }

        public static T GetRandom<T>(this IEnumerable<T> list, Random random) where T : IWithProbability
        {
            var value = random.NextDouble();
            var shift = 0d;
            foreach (var item in list)
            {
                shift += item.Probability;
                if (shift > 1)
                {
                    throw new ArgumentException($"Sum of probability must be 1, but is {list.Sum(l => l.Probability)}");
                }
                if (shift > value)
                {
                    return item;
                }
            }
            throw new ArgumentException($"Sum of probability must be 1, but is {list.Sum(l => l.Probability)}");
        }

        public static double GetDensity(this IWithDensity densityDefinition, Random random)
        {
            if (densityDefinition.MaxDensity == densityDefinition.MinDensity)
            {
                return densityDefinition.MinDensity;
            }
            return densityDefinition.MinDensity + (densityDefinition.MaxDensity - densityDefinition.MinDensity) * random.NextDouble();
        }

    }
}
