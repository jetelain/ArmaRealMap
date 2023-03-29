namespace GameRealisticMap.Roads
{
    public class RoadsData : ITerrainData
    {
        public RoadsData(List<Road> roads)
        { 
            Roads = roads;
        }

        public List<Road> Roads { get; }
    }
}
