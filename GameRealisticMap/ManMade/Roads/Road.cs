using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads.Libraries;

namespace GameRealisticMap.ManMade.Roads
{
    public class Road : ITerrainEnvelope, IWay
    {
        [JsonConstructor]
        public Road(WaySpecialSegment specialSegment, TerrainPath path, IRoadTypeInfos roadTypeInfos)
        {
            SpecialSegment = specialSegment;
            Path = path;
            RoadTypeInfos = roadTypeInfos;
            MinPoint = path.MinPoint - new Vector2(roadTypeInfos.Width);
            MaxPoint = path.MaxPoint + new Vector2(roadTypeInfos.Width);
        }

        public WaySpecialSegment SpecialSegment { get; }

        public TerrainPath Path { get; set; }

        public IRoadTypeInfos RoadTypeInfos { get; }

        [JsonIgnore]
        public RoadTypeId RoadType => RoadTypeInfos.Id;

        [JsonIgnore]
        public float Width => RoadTypeInfos.Width;

        [JsonIgnore]
        public float ClearWidth => RoadTypeInfos.ClearWidth;

        [JsonIgnore]
        public TerrainPoint MinPoint { get; }

        [JsonIgnore]
        public TerrainPoint MaxPoint { get; }

        [JsonIgnore]
        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        [JsonIgnore]
        public IEnumerable<TerrainPolygon> ClearPolygons => Path.ToTerrainPolygon(ClearWidth);

        [JsonIgnore]
        public float Factor
        {
            get
            {
                switch (RoadType) 
                {
                    case RoadTypeId.TwoLanesPrimaryRoad:
                        return 0.2f;
                    case RoadTypeId.TwoLanesSecondaryRoad:
                        return 0.4f;
                    case RoadTypeId.TwoLanesConcreteRoad:
                        return 0.6f;
                    default: 
                        return 1f;
                }
            }
        }
    }
}
