using System.Text.Json.Serialization;

namespace GameRealisticMap.Arma3.Assets
{
    public sealed class TerrainMaterialData
    {
        [JsonConstructor]
        public TerrainMaterialData(TerrainMaterialDataFormat format, byte[] colorTexture, byte[] normalTexture)
        {
            Format = format;
            ColorTexture = colorTexture;
            NormalTexture = normalTexture;
        }

        public TerrainMaterialDataFormat Format { get; }

        public byte[] ColorTexture { get; }

        public byte[] NormalTexture { get; }
    }
}