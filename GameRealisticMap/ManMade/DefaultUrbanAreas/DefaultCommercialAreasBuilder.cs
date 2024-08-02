using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultCommercialAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultCommercialAreaData>
    {
        protected override BuildingTypeId TragetedType => BuildingTypeId.Commercial;

        protected override DefaultCommercialAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultCommercialAreaData(polygons);
        }
    }
}
