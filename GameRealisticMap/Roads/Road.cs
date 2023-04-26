using GameRealisticMap.Geometries;

namespace GameRealisticMap.Roads
{
    public class Road : ITerrainEnvelope
    {
        public Road(RoadSpecialSegment specialSegment, TerrainPath path, IRoadTypeInfos roadTypeInfos)
        {
            SpecialSegment = specialSegment;
            Path = path;
            RoadTypeInfos = roadTypeInfos;
        }

        public RoadSpecialSegment SpecialSegment { get; }

        public TerrainPath Path { get; set; }

        public IRoadTypeInfos RoadTypeInfos { get; }

        public RoadTypeId RoadType => RoadTypeInfos.Id;

        public float Width => RoadTypeInfos.Width;

        public float ClearWidth => RoadTypeInfos.ClearWidth;

        public TerrainPoint MinPoint => Path.MinPoint;

        public TerrainPoint MaxPoint => Path.MaxPoint;

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
