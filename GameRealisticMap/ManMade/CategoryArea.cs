using GameRealisticMap.Buildings;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade
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
