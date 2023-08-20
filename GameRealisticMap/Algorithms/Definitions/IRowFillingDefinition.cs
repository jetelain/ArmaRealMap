namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IRowFillingDefinition<out TModel>
        : IRowDefition<TModel>
    {
        double RowSpacing { get; }
    }
}
