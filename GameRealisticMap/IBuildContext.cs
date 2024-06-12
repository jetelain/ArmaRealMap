using GameRealisticMap.Osm;

namespace GameRealisticMap
{
    public interface IBuildContext : IContext
    {
        ITerrainArea Area { get; }

        IOsmDataSource OsmSource { get; }

        IMapProcessingOptions Options { get; }
    }
}