using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Buildings
{
    public class Building
    {
        public Building(BoundingBox box, BuildingTypeId value, List<TerrainPolygon> polygons)
        {
            Box = box;
            Value = value;
            Polygons = polygons;
        }

        public BoundingBox Box { get; }
        public BuildingTypeId Value { get; }
        public List<TerrainPolygon> Polygons { get; }
    }
}
