namespace GameRealisticMap.Geometries
{
    public interface ITerrainEnvelope
    {
        TerrainPoint MinPoint { get; }

        TerrainPoint MaxPoint { get; }
    }
}
