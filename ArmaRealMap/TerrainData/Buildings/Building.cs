using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ArmaRealMap.Core.ObjectLibraries;
using GameRealisticMap.Geometries;
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
            Box = json.Box;
            Category = json.Category;
        }

        [JsonConstructor]
        public Building(BoundingBox box, ObjectCategory? category)
        {
            Box = box;
            Category = category;
        }

        [JsonIgnore]
        public List<OsmShape> Shapes { get; }

        public BoundingBox Box { get; set; }

        public ObjectCategory? Category { get; set; }

        [JsonIgnore]
        public Polygon Poly => Box.Poly;

        [JsonIgnore]
        public TerrainPoint MinPoint => Box.MinPoint;

        [JsonIgnore]
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
                Box = Box
            };
        }
    }
}