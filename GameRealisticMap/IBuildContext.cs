using GameRealisticMap.Osm;

namespace GameRealisticMap
{
    public interface IBuildContext
    {
        ITerrainArea Area { get; }

        IOsmDataSource OsmSource { get; }

        T GetData<T>() where T : class, ITerrainData;
    }
}