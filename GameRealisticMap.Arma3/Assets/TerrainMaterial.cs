using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.IO.Converters;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterial
    {
        public TerrainMaterial(string normalTexture, string colorTexture, Rgb24 id, byte[]? pngFakeSat)
        {
            NormalTexture = normalTexture;
            ColorTexture = colorTexture;
            Id = id;
            FakeSatPngImage = pngFakeSat;
        }

        public string NormalTexture { get; }

        public string ColorTexture { get; }

        [JsonConverter(typeof(Rgb24Converter))]
        public Rgb24 Id { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public byte[]? FakeSatPngImage { get; }
    }
}