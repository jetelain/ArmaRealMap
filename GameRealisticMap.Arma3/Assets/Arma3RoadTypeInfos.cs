using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.IO.Converters;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Assets
{
    public sealed class Arma3RoadTypeInfos : IRoadTypeInfos, IArma3RoadTypeInfos
    {
        [JsonConstructor]
        public Arma3RoadTypeInfos(RoadTypeId id, Rgb24 satelliteColor, float textureWidth, string texture, string textureEnd, string material, float width, float clearWidth, StreetLampsCondition? proceduralStreetLamps = null, float? distanceBetweenStreetLamps = null, float? distanceBetweenStreetLampsMax = null, bool? hasSideWalks = null)
        {
            SatelliteColor = satelliteColor;
            TextureWidth = textureWidth;
            Texture = texture;
            TextureEnd = textureEnd;
            Material = material;
            Id = id;
            Width = width;
            ClearWidth = clearWidth;
            ProceduralStreetLamps = proceduralStreetLamps ?? DefaultRoadTypeLibrary.Instance.GetInfo(id).ProceduralStreetLamps;
            if ((ProceduralStreetLamps ?? StreetLampsCondition.None) != StreetLampsCondition.None)
            {
                var min = distanceBetweenStreetLamps ?? (ClearWidth * 2.5f);
                var max = distanceBetweenStreetLampsMax ?? min;
                DistanceBetweenStreetLamps = Math.Min(min, max);
                DistanceBetweenStreetLampsMax = Math.Max(min, max);
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

        public StreetLampsCondition? ProceduralStreetLamps { get; }

        public float? DistanceBetweenStreetLamps { get; }

        public float? DistanceBetweenStreetLampsMax { get; }

        public bool? HasSideWalks { get; }

        string IArma3RoadTypeInfos.Map
        {
            get
            {
                switch (Id)
                {
                    case RoadTypeId.TwoLanesMotorway:
                    case RoadTypeId.TwoLanesPrimaryRoad:
                        return "main road";

                    case RoadTypeId.TwoLanesSecondaryRoad:
                        return "road";

                    case RoadTypeId.TwoLanesConcreteRoad:
                    case RoadTypeId.SingleLaneDirtRoad:
                    case RoadTypeId.SingleLaneDirtPath:
                    default:
                        return "track";

                    case RoadTypeId.Trail:
                    case RoadTypeId.ConcreteFootway:
                        return "trail";
                }
            }
        }

        float IArma3RoadTypeInfos.PathOffset
        {
            get
            {
                switch (Id)
                {
                    case RoadTypeId.TwoLanesMotorway:
                    case RoadTypeId.TwoLanesPrimaryRoad:
                        return 1;
                    case RoadTypeId.TwoLanesSecondaryRoad:
                    case RoadTypeId.TwoLanesConcreteRoad:
                        return 1.5f;
                    case RoadTypeId.SingleLaneDirtRoad:
                        return 2;
                    case RoadTypeId.SingleLaneDirtPath:
                        return 2.5f;
                    case RoadTypeId.Trail:
                    case RoadTypeId.ConcreteFootway:
                    default:
                        return 0;
                }
            }
        }

        bool IArma3RoadTypeInfos.IsPedestriansOnly => Id == RoadTypeId.Trail || Id == RoadTypeId.ConcreteFootway;

        StreetLampsCondition IRoadTypeInfos.ProceduralStreetLamps => ProceduralStreetLamps ?? StreetLampsCondition.None;

        float IRoadTypeInfos.DistanceBetweenStreetLamps => DistanceBetweenStreetLamps ?? (ClearWidth * 2.5f);

        bool IRoadTypeInfos.HasSideWalks => HasSideWalks ?? false;

        float IRoadTypeInfos.DistanceBetweenStreetLampsMax => DistanceBetweenStreetLampsMax ?? DistanceBetweenStreetLamps ?? (ClearWidth * 2.5f);

        int IArma3RoadTypeInfos.Id => ((int)Id) + 1;
    }
}
