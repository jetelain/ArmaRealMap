namespace GameRealisticMap.Algorithms.Definitions
{
    public interface IDensityDefinition
    {
        IWithDensity Default { get; }

        IDensityWithNoiseDefinition? LargeAreas { get; }
    }
}
