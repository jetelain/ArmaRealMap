using GameRealisticMap.Geometries;

namespace GameRealisticMap.Buildings
{
    public class Building : ITerrainGeometry
    {
        public Building(BoundingBox box, BuildingTypeId value, List<TerrainPolygon> polygons)
        {
            Box = box;
            TypeId = value;
            Polygons = polygons;
        }

        public BoundingBox Box { get; }

        public BuildingTypeId TypeId { get; }

        public List<TerrainPolygon> Polygons { get; }

        public TerrainPoint MinPoint => Box.MinPoint;

        public TerrainPoint MaxPoint => Box.MaxPoint;
    }
}
