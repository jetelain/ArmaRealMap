namespace GameRealisticMap.ManMade.Roads.Libraries
{
    public interface IRoadTypeLibrary<out T> where T : IRoadTypeInfos
    {
        IBridgeInfos GetBridge(RoadTypeId id);

        T GetInfo(RoadTypeId id);
    }
}
