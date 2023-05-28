using GameRealisticMap.Algorithms.Definitions;

namespace GameRealisticMap.Algorithms
{
    public static class DensityHelper
    {
        public static double GetMaxDensity<TModelInfo>(IEnumerable<IClusterItemDefinition<TModelInfo>> items)
        {
            return 1 / items.Sum(o => o.Probability * Math.Pow(o.Radius, 2) * Math.PI) * 0.8d;
        }

        public static double GetMaxDensity<TModelInfo>(IEnumerable<IClusterDefinition<TModelInfo>> clusters)
        {
            return 1 / clusters.Sum(c => c.Models.Sum(o => o.Probability * c.Probability * Math.Pow(o.Radius, 2) * Math.PI) * 0.8d);
        }
    }
}
