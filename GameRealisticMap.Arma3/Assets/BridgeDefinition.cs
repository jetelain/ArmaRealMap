namespace GameRealisticMap.Arma3.Assets
{
    public class BridgeDefinition
    {
        public BridgeDefinition(BridgeSegmentDefinition single, BridgeSegmentDefinition start, BridgeSegmentDefinition middle, BridgeSegmentDefinition end)
        {
            Single = single;
            Start = start;
            Middle = middle;
            End = end;
        }

        public BridgeSegmentDefinition Single { get; }

        public BridgeSegmentDefinition Start { get; }

        public BridgeSegmentDefinition Middle { get; }

        public BridgeSegmentDefinition End { get; }

    }
}