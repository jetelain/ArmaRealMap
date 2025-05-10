using GameRealisticMap.Osm;

namespace GameRealisticMap
{
    public interface IBuildContext : IContext
    {
        IOsmDataSource OsmSource { get; }

        IMapProcessingOptions Options { get; }
    }
}