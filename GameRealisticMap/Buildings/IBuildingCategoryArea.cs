using GameRealisticMap.Geometries;
using NetTopologySuite.Geometries;

namespace GameRealisticMap.Buildings
{
    internal interface IBuildingCategoryArea
    {
        BuildingTypeId BuildingType { get; }

        IEnumerable<TerrainPolygon> PolyList { get; }
    }
}