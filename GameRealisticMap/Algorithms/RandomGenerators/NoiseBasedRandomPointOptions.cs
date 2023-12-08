using System.Text.Json.Serialization;

namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public sealed class NoiseBasedRandomPointOptions : INoiseBasedRandomPointOptions
    {
        public static readonly NoiseBasedRandomPointOptions Default = new NoiseBasedRandomPointOptions();

        [JsonConstructor]
        public NoiseBasedRandomPointOptions(int? seed= null, float threshold = 1, int samples = 10, float frequency = 0.01f, FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin)
        {
            Seed = seed;
            Threshold = threshold;
            Samples = samples;
            Frequency = frequency;
            NoiseType = noiseType;
        }

        public int? Seed { get; }

        public float Threshold { get; }

        public int Samples { get; }

        public float Frequency { get; }

        public FastNoiseLite.NoiseType NoiseType { get; }
    }
}