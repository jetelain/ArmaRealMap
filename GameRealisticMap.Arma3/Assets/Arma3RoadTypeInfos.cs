﻿using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.IO.Converters;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Assets
{
    public class Arma3RoadTypeInfos : IRoadTypeInfos
    {
        [JsonConstructor]
        public Arma3RoadTypeInfos(RoadTypeId id, Rgb24 satelliteColor, float textureWidth, string texture, string textureEnd, string material, float width, float clearWidth)
        {
            SatelliteColor = satelliteColor;
            TextureWidth = textureWidth;
            Texture = texture;
            TextureEnd = textureEnd;
            Material = material;
            Id = id;
            Width = width;
            ClearWidth = clearWidth;
        }

        [JsonConverter(typeof(Rgb24Converter))]
        public Rgb24 SatelliteColor { get; }

        public float TextureWidth { get; }

        public string Texture { get; }

        public string TextureEnd { get; }

        public string Material { get; }

        public RoadTypeId Id { get; }

        public float Width { get; }

        public float ClearWidth { get; }
    }
}