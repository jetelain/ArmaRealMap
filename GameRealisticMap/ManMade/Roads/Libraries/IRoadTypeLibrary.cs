namespace GameRealisticMap.ManMade.Roads.Libraries
{
    public interface IRoadTypeLibrary<out T> where T : IRoadTypeInfos
    {
        T GetInfo(RoadTypeId id);
    }
}
