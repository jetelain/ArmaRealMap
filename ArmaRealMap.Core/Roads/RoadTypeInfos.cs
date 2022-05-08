namespace ArmaRealMap.Core.Roads
{
    public class RoadTypeInfos
    {
        public RoadTypeId Id { get; set; }
        public TerrainRegion? Terrain { get; set; }
        public float Width { get; set; }
        public float TextureWidth { get; set; }
        public string Texture { get; set; }
        public string TextureEnd { get; set; }
        public string Material { get; set; }
        public string SatelliteColor { get; set; }
    }
}
