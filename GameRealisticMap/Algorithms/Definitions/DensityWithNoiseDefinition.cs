using System.Text.Json.Serialization;
using GameRealisticMap.Algorithms.RandomGenerators;

namespace GameRealisticMap.Algorithms.Definitions
{
    public sealed class DensityWithNoiseDefinition : IDensityWithNoiseDefinition
    {
        [JsonConstructor]
        public DensityWithNoiseDefinition(double minDensity, double maxDensity, double noiseProportion = 0, NoiseBasedRandomPointOptions? noiseOptions = null)
        {
            MinDensity = minDensity;
            MaxDensity = maxDensity;
            NoiseProportion = noiseProportion;
            NoiseOptions = noiseOptions;
        }

        public double MinDensity { get; }

        public double MaxDensity { get; }

        public double NoiseProportion { get; }

        public NoiseBasedRandomPointOptions? NoiseOptions { get; }

        INoiseBasedRandomPointOptions? IRandomPointGenerationOptions.NoiseOptions => NoiseOptions;
    }
}
