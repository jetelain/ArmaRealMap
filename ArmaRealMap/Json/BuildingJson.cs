using ArmaRealMap.Core.ObjectLibraries;
using GameRealisticMap.Geometries;

namespace ArmaRealMap
{
    public class BuildingJson
    {
        public ObjectCategory? Category { get; set; }
        public BoundingBox Box { get; set; }
    }
}