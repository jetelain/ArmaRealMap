namespace GameRealisticMap.Arma3.Assets
{
    public class BridgeDefinition
    {
        public BridgeDefinition(StraightSegmentDefinition single, StraightSegmentDefinition start, StraightSegmentDefinition middle, StraightSegmentDefinition end)
        {
            Single = single;
            Start = start;
            Middle = middle;
            End = end;
        }

        public StraightSegmentDefinition Single { get; }

        public StraightSegmentDefinition Start { get; }

        public StraightSegmentDefinition Middle { get; }

        public StraightSegmentDefinition End { get; }

    }
}