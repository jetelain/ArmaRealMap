namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IRowDefition<out TModel> : IWithProbability
    {
        IReadOnlyCollection<IStraightSegmentProportionDefinition<TModel>> Straights { get; }

        IReadOnlyCollection<IItemDefinition<TModel>> Objects { get; }

    }
}
