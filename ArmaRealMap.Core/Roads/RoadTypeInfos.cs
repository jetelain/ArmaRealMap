using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace ArmaRealMap.Core.Roads
{
    public class RoadTypeInfos : IRoadTypeInfos
    {
        public RoadTypeId Id { get; set; }
        public TerrainRegion? Terrain { get; set; }
        public float Width { get; set; }
        public float TextureWidth { get; set; }
        public string Texture { get; set; }
        public string TextureEnd { get; set; }
        public string Material { get; set; }
        public string SatelliteColor { get; set; }

        public float ClearWidth
        {
            get
            {
                if (Id < RoadTypeId.TwoLanesPrimaryRoad)
                {
                    return Width + 6f;
                }
                if (Id < RoadTypeId.SingleLaneDirtPath)
                {
                    return Width + 4f;
                }
                return Width + 2f;
            }
        }
    }
}
