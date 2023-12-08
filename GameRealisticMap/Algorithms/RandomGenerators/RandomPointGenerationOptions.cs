namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public sealed class RandomPointGenerationOptions : IRandomPointGenerationOptions
    {
        public static readonly RandomPointGenerationOptions Default = new RandomPointGenerationOptions();

        public RandomPointGenerationOptions(double noiseProportion = 0, INoiseBasedRandomPointOptions? noiseOptions = null)
        {
            NoiseProportion = noiseProportion;
            NoiseOptions = noiseOptions;
        }

        public double NoiseProportion { get; }

        public INoiseBasedRandomPointOptions? NoiseOptions { get; }
    }
}
