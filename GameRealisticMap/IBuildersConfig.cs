using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap
{
    public interface IBuildersConfig
    {
        IRoadTypeLibrary<IRoadTypeInfos> Roads { get; }

        IRailwayCrossingResolver RailwayCrossings { get; }
    }
}
