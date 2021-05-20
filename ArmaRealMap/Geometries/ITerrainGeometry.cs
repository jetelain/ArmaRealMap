namespace ArmaRealMap.Geometries
{
    public interface ITerrainGeometry
    {
        TerrainPoint MinPoint { get; }

        TerrainPoint MaxPoint { get; }
    }
}
