namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public interface IRandomPointGenerationOptions
    {
        double NoiseProportion { get; }

        INoiseBasedRandomPointOptions? NoiseOptions { get; }
    }
}