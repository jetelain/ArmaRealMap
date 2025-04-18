﻿using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterial
    {
        [JsonConstructor]
        public TerrainMaterial(string normalTexture, string colorTexture, Rgb24 id, byte[]? fakeSatPngImage)
        {
            NormalTexture = normalTexture;
            ColorTexture = colorTexture;
            Id = id;
            FakeSatPngImage = fakeSatPngImage;
        }

        public string NormalTexture { get; }

        public string ColorTexture { get; }

        [JsonConverter(typeof(Rgb24Converter))]
        public Rgb24 Id { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public byte[]? FakeSatPngImage { get; }

        public string GetNormalTexturePath(IArma3MapConfig context)
        {
            return Format(NormalTexture, context);
        }

        public string GetColorTexturePath(IArma3MapConfig context)
        {
            return Format(ColorTexture, context);
        }

        internal static string Format(string texture, IArma3MapConfig context)
        {
            return texture
                .Replace("{PboPrefix}", context.PboPrefix);
        }
    }
}