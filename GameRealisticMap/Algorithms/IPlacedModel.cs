namespace GameRealisticMap.Algorithms
{
    public interface IPlacedModel<out TModelInfo> : IModelPosition
    {
        TModelInfo Model { get; }
    }
}
