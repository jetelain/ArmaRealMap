using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

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
