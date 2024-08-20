using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultAgriculturalAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultAgriculturalAreaData>
    {
        protected override BuildingTypeId TragetedType => BuildingTypeId.Agricultural;

        protected override DefaultAgriculturalAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultAgriculturalAreaData(polygons);
        }
    }
}
