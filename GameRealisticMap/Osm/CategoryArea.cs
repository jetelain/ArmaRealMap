using GameRealisticMap.Buildings;
using GameRealisticMap.Geometries;
using NetTopologySuite.Geometries;

namespace GameRealisticMap.Osm
{
    internal class CategoryArea : IBuildingCategoryArea
    {
        public CategoryArea(BuildingTypeId buildingType, List<TerrainPolygon> polyList)
        {
            BuildingType = buildingType;
            PolyList = polyList;
        }

        public BuildingTypeId BuildingType { get; }

        public IEnumerable<TerrainPolygon> PolyList { get; }
    }
}
