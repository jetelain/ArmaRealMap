using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultIndustrialAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultIndustrialAreaData>
    {
        protected override BuildingTypeId TragetedType => BuildingTypeId.Industrial;

        protected override DefaultIndustrialAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultIndustrialAreaData(polygons);
        }
    }
}
