using GameRealisticMap.Geometries;

namespace GameRealisticMap.Roads
{
    public class Road : ITerrainGeometry
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

        public TerrainPoint MinPoint => Path.MinPoint;

        public TerrainPoint MaxPoint => Path.MaxPoint;

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);
    }
}
