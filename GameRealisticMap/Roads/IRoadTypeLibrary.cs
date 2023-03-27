namespace GameRealisticMap.Roads
{
    public interface IRoadTypeLibrary
    {
        IRoadTypeInfos GetInfo(RoadTypeId id);
    }
}
