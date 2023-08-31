using System.Text.Json.Serialization;
using GameRealisticMap.IO.Converters;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Assets
{
    public class Arma3RoadTypeInfos : IRoadTypeInfos
    {
        [JsonConstructor]
        public Arma3RoadTypeInfos(RoadTypeId id, Rgb24 satelliteColor, float textureWidth, string texture, string textureEnd, string material, float width, float clearWidth, bool? hasStreetLamp = null, float? distanceBetweenStreetLamps = null, bool? hasSideWalks = null)
        {
            SatelliteColor = satelliteColor;
            TextureWidth = textureWidth;
            Texture = texture;
            TextureEnd = textureEnd;
            Material = material;
            Id = id;
            Width = width;
            ClearWidth = clearWidth;
            HasStreetLamp = hasStreetLamp ?? DefaultRoadTypeLibrary.Instance.GetInfo(id).HasStreetLamp;
            if (HasStreetLamp ?? false)
            {
                DistanceBetweenStreetLamps = distanceBetweenStreetLamps ?? (ClearWidth * 2.5f);
            }
            HasSideWalks = hasSideWalks ?? DefaultRoadTypeLibrary.Instance.GetInfo(id).HasSideWalks;
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

        public bool? HasStreetLamp { get; }

        public float? DistanceBetweenStreetLamps { get; }

        public bool? HasSideWalks { get; }

        bool IRoadTypeInfos.HasStreetLamp => HasStreetLamp ?? false;

        float IRoadTypeInfos.DistanceBetweenStreetLamps => DistanceBetweenStreetLamps ?? (ClearWidth * 2.5f);

        bool IRoadTypeInfos.HasSideWalks => HasSideWalks ?? false;
    }
}
