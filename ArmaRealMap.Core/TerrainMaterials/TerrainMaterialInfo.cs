namespace ArmaRealMap.Core.TerrainMaterials
{
    public class TerrainMaterialInfo
    {
        public TerrainMaterialId Id { get; set; }
        public string ColorTexture { get; set; }
        public string NormalTexture { get; set; }
        public byte[] FakeSatPngImage { get; set; }
        public TerrainRegion? Terrain { get; set; }
    }
}
