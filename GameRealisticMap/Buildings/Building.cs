using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Buildings
{
    public class Building : ITerrainEnvelope
    {
        [JsonConstructor]
        public Building(BoundingBox box, BuildingTypeId typeId, List<TerrainPolygon> polygons, BoxSide entranceSide)
        {
            Box = box;
            TypeId = typeId;
            Polygons = polygons;
            EntranceSide = entranceSide;
        }

        public BoundingBox Box { get; }

        public BuildingTypeId TypeId { get; }

        public List<TerrainPolygon> Polygons { get; }

        public BoxSide EntranceSide { get; }

        [JsonIgnore]
        public TerrainPoint MinPoint => Box.MinPoint;

        [JsonIgnore]
        public TerrainPoint MaxPoint => Box.MaxPoint;
    }
}
