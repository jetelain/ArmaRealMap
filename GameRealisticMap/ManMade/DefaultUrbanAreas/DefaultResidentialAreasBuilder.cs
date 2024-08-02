using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultResidentialAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultResidentialAreaData>
    {
        protected override BuildingTypeId TragetedType => BuildingTypeId.Residential;

        protected override DefaultResidentialAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultResidentialAreaData(polygons);
        }
    }
}
