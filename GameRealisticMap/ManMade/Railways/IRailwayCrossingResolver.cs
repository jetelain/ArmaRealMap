using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.ManMade.Railways
{
    public interface IRailwayCrossingResolver
    {
        float GetCrossingWidth(IRoadTypeInfos? roadTypeInfos, float factor);
    }
}
