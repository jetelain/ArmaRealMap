namespace GameRealisticMap.Geometries
{
    public interface ITerrainGeo : ITerrainEnvelope
    {
        float Distance(TerrainPoint p);

        TerrainPoint NearestPointBoundary(TerrainPoint p);
    }
}
