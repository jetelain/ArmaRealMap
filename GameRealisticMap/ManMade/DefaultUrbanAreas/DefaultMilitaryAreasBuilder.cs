using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultMilitaryAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultMilitaryAreaData>
    {
        protected override BuildingTypeId TragetedType => BuildingTypeId.Military;

        protected override DefaultMilitaryAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultMilitaryAreaData(polygons);
        }
    }
}
