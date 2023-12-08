namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public interface INoiseOptions
    {
        int? Seed { get; }

        float Frequency { get; }

        FastNoiseLite.NoiseType NoiseType { get; }
    }
}
