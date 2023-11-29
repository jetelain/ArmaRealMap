namespace GameRealisticMap.Algorithms.RandomGenerators
{
    public interface INoiseBasedRandomPointOptions : INoiseOptions
    {
        float Threshold { get; }

        int Samples { get; }
    }
}
