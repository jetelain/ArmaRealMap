namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IStraightSegmentDefinition<out TModelInfo>
    {
        TModelInfo Model { get; }

        float Size { get; }
    }
}
