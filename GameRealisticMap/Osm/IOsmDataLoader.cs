namespace GameRealisticMap.Osm
{
    public interface IOsmDataLoader
    {
        Task<IOsmDataSource> Load(ITerrainArea area);
    }
}