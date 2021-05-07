using System.Collections.Generic;
using System.Linq;
using ArmaRealMap.Geometries;
using NetTopologySuite.Geometries;

namespace ArmaRealMap
{
    internal class Building
    {
        public Building(Area area, TerrainPoint[] points)
        {
            Areas = new List<Area>() { area };
            Box = BoundingBox.Compute(points);
            Category = area.BuildingCategory;
        }

        public Building(BuildingJson json)
        {
            Box = new BoundingBox(json.Box);
            Category = json.Category;
        }

        public List<Area> Areas { get; }

        public BoundingBox Box { get; private set; }

        public ObjectCategory? Category { get; set; }

        public Polygon Poly => Box.Poly;

        public void Add(Building other)
        {
            Areas.AddRange(other.Areas);
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