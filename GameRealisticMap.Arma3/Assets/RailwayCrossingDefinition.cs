namespace GameRealisticMap.Arma3.Assets
{
    public class RailwayCrossingDefinition : StraightSegmentDefinition
    {
        public RailwayCrossingDefinition(Composition model, float size, float roadMaxSize)
            : base(model, size)
        {
            RoadMaxSize = roadMaxSize;
        }

        public float RoadMaxSize { get; }
    }
}
