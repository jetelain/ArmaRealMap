using System;
using System.Linq;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Conditions;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms
{
    public static class RandomHelper
    {
        public static Random CreateRandom(TerrainPoint seed)
        {
            return new Random((int)Math.Truncate(seed.X + seed.Y));
        }

        public static void CheckProbabilitySum<T>(this IReadOnlyCollection<T> list) where T : IWithProbability
        {
            if (list.Count == 0)
            {
                return;
            }
            var sum = list.Sum(l => l.Probability);
            if (Math.Abs(sum - 1) > 0.001)
            {
                throw new ArgumentException($"Sum of probability must be 1, but is {sum}");
            }
        }

        public static T GetRandom<T>(this IReadOnlyCollection<T> list, TerrainPoint seed) where T : IWithProbability
        {
            return GetRandom<T>(list, CreateRandom(seed));
        }

        public static T GetRandom<T>(this IReadOnlyCollection<T> list, Random random) where T : IWithProbability
        {
            if (list.Count == 1)
            {
                return list.First();
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
                if (shift > 1.001)
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

        public static T GetEquiprobale<T>(this IReadOnlyList<T> list, Random random)
        {
            return list[random.Next(0, list.Count)];
        }

        public static double GetDensity(this IWithDensity densityDefinition, Random random)
        {
            if (densityDefinition.MaxDensity == densityDefinition.MinDensity)
            {
                return densityDefinition.MinDensity;
            }
            return densityDefinition.MinDensity + (densityDefinition.MaxDensity - densityDefinition.MinDensity) * random.NextDouble();
        }

        public static T? GetRandomWithProportion<T>(this IEnumerable<T> list, Random random) where T : IWithProportion
        {
            var value = random.NextDouble();
            var matching = list.ToList();
            if (matching.Count == 1)
            {
                return matching[0];
            }
            if (matching.Count == 0)
            {
                return default(T);
            }
            var sumOfProportions = matching.Sum(i => i.Proportion);
            var shift = 0d;
            foreach (var item in matching)
            {
                shift += item.Proportion / sumOfProportions;
                if (shift > value)
                {
                    return item;
                }
            }
            return default(T);
        }

        public static float GetScale<TModelInfo>(this IItemDefinition<TModelInfo> obj, Random random)
        {
            if (obj.MinScale != null && obj.MaxScale != null)
            {
                return (float)(obj.MinScale + ((obj.MaxScale - obj.MinScale) * random.NextDouble()));
            }
            return 1;
        }

        public static float GetElevation<TModelInfo>(this IItemDefinition<TModelInfo> obj, Random random)
        {
            if (obj.MaxZ != null && obj.MinZ != null)
            {
                return (float)(obj.MinZ + ((obj.MaxZ - obj.MinZ) * random.NextDouble()));
            }
            return 0;
        }

        public static float GetAngle(this Random random)
        {
            return (float)(random.NextDouble() * 360);
        }

        public static TItem? GetRandom<TItem>(this IReadOnlyCollection<TItem> list, TerrainPoint point, IPointConditionContext context)
            where TItem : class, IWithProbabilityAndCondition<IPointConditionContext>
        {
            return GetRandom<TItem, IPointConditionContext>(list, CreateRandom(point), context);
        }

        public static TItem? GetRandom<TItem, TContext>(this IReadOnlyCollection<TItem> list, Random random, TContext context)
            where TItem : class, IWithProbabilityAndCondition<TContext>
        {
            var optimist = list.GetRandom<TItem>(random);
            if (optimist.Condition == null || optimist.Condition.Evaluate(context))
            {
                return optimist;
            }
            var filtered = EvaluateOtherItems(list, context, optimist);
            if (filtered.Count == 0)
            {
                return default;
            }
            if (filtered.Count == 1)
            {
                return filtered[0];
            }
            var value = random.NextDouble() * filtered.Sum(f => f.Probability);
            var shift = 0d;
            foreach (var item in list)
            {
                shift += item.Probability;
                if (shift > value)
                {
                    return item;
                }
            }
            throw new InvalidOperationException();
        }

        public static TItem? GetEquiprobale<TItem, TContext>(this IReadOnlyList<TItem> list, Random random, TContext context)
            where TItem : class, IWithProbabilityAndCondition<TContext>
        {
            var optimist = list.GetEquiprobale<TItem>(random);
            if (optimist.Condition == null || optimist.Condition.Evaluate(context))
            {
                return optimist;
            }
            var filtered = EvaluateOtherItems(list, context, optimist);
            if (filtered.Count == 0)
            {
                return default;
            }
            return filtered[random.Next(0, filtered.Count)];
        }

        private static List<TItem> EvaluateOtherItems<TItem, TContext>(IReadOnlyCollection<TItem> list, TContext context, TItem ignore) 
            where TItem : class, IWithProbabilityAndCondition<TContext>
        {
            return list.Where(i => i != ignore && (i.Condition == null || i.Condition.Evaluate(context))).ToList();
        }
    }
}
