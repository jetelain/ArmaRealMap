using GameRealisticMap.Geometries;

namespace GameRealisticMap.Buildings
{
    public class Building : ITerrainGeometry
    {
        public Building(BoundingBox box, BuildingTypeId value, List<TerrainPolygon> polygons, BoxSide entranceSide)
        {
            Box = box;
            TypeId = value;
            Polygons = polygons;
            EntranceSide = entranceSide;
        }

        public BoundingBox Box { get; }

        public BuildingTypeId TypeId { get; }

        public List<TerrainPolygon> Polygons { get; }

        public BoxSide EntranceSide { get; }

        public TerrainPoint MinPoint => Box.MinPoint;

        public TerrainPoint MaxPoint => Box.MaxPoint;
    }
}
