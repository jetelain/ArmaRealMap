using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Buildings
{
    internal interface IBuildingCategoryArea
    {
        BuildingTypeId BuildingType { get; }

        IEnumerable<TerrainPolygon> PolyList { get; }
    }
}