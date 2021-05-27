using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Geometries;
using ArmaRealMap.Osm;
using NetTopologySuite.Geometries;

namespace ArmaRealMap
{
    internal class Building : ITerrainGeometry
    {
        public Building(OsmShape area, TerrainPoint[] points)
        {
            Shapes = new List<OsmShape>() { area };
            Box = BoundingBox.Compute(points);
            Category = area.BuildingCategory;
        }

        public Building(BuildingJson json)
        {
            Box = new BoundingBox(json.Box);
            Category = json.Category;
        }

        public List<OsmShape> Shapes { get; }

        public BoundingBox Box { get; private set; }

        public ObjectCategory? Category { get; set; }

        public Polygon Poly => Box.Poly;

        public TerrainPoint MinPoint => Box.MinPoint;

        public TerrainPoint MaxPoint => Box.MaxPoint;

        public void Add(Building other)
        {
            Shapes.AddRange(other.Shapes);
            Box = Box.Add(other.Box);
            Category = Category ?? other.Category;
        }

        public BuildingJson ToJson()
        {
            return new BuildingJson()
            {
                Category = Category,
                Box = Box.ToJson()
            };
        }
    }
}