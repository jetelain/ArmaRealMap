using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap
{
    public class DefaultBuildersConfig : IBuildersConfig
    {
        public IRoadTypeLibrary<IRoadTypeInfos> Roads { get; } = new DefaultRoadTypeLibrary();

        public IRailwayCrossingResolver RailwayCrossings { get; } = new DefaultRailwayCrossingResolver();
    }
}
