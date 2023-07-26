using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.ManMade.Railways
{
    public class DefaultRailwayCrossingResolver : IRailwayCrossingResolver
    {
        public float GetCrossingWidth(IRoadTypeInfos? roadTypeInfos, float factor)
        {
            return (roadTypeInfos?.ClearWidth ?? 6f) * factor;
        }
    }
}
