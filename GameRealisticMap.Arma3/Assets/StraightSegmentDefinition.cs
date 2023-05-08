namespace GameRealisticMap.Arma3.Assets
{
    /// <summary>
    /// Straight segment, aligned to north, centered on (0,0)
    /// </summary>
    public class StraightSegmentDefinition
    {
        public StraightSegmentDefinition(Composition composition, float size)
        {
            Composition = composition;
            Size = size;
        }

        public Composition Composition { get; }

        public float Size { get; }
    }
}
