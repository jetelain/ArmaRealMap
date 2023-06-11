using GameRealisticMap.Geometries;
using GeoAPI.Geometries;

namespace GameRealisticMap.ManMade.Buildings
{
    internal class BuildingCandidate : ITerrainEnvelope
    {
        public BuildingCandidate(TerrainPolygon polygon, BuildingTypeId? category)
        {
            Polygons = new List<TerrainPolygon>() { polygon };
            Box = BoundingBox.Compute(polygon.Shell.ToArray());
            Category = category;
        }

        public List<TerrainPolygon> Polygons { get; }

        public BoundingBox Box { get; set; }

        public IPolygon Poly => Box.Poly;

        public TerrainPolygon Polygon => Box.Polygon;

        public BuildingTypeId? Category { get; set; }

        public BoxSide EntranceSide { get; set; }

        public TerrainPoint MinPoint => Box.MinPoint;

        public TerrainPoint MaxPoint => Box.MaxPoint;

        public void AddAndMerge(BuildingCandidate other)
        {
            Polygons.AddRange(other.Polygons);
            Box = Box.Add(other.Box);
            Category = Category ?? other.Category;
        }

        internal Building ToBuilding()
        {
            if (Category == null)
            {
                throw new InvalidOperationException();
            }
            return new Building(Box, Category.Value, Polygons, EntranceSide);
        }
    }
}