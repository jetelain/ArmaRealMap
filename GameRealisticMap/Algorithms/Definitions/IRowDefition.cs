namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IRowDefition<out TModel> : IWithProbability
    {
        IReadOnlyCollection<IStraightSegmentProportionDefinition<TModel>> Segments { get; }

        IReadOnlyCollection<IItemDefinition<TModel>> Objects { get; }

    }
}
