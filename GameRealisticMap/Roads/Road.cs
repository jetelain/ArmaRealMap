using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;

namespace GameRealisticMap.Roads
{
    public class Road : ITerrainEnvelope, IWay
    {
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

        public RoadTypeId RoadType => RoadTypeInfos.Id;

        public float Width => RoadTypeInfos.Width;

        public float ClearWidth => RoadTypeInfos.ClearWidth;

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public IEnumerable<TerrainPolygon> ClearPolygons => Path.ToTerrainPolygon(ClearWidth);

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
