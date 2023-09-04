using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultCommercialAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultCommercialAreaData>
    {
        public DefaultCommercialAreasBuilder(IProgressSystem progress) : base(progress)
        {
        }

        protected override BuildingTypeId TragetedType => BuildingTypeId.Commercial;

        protected override DefaultCommercialAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultCommercialAreaData(polygons);
        }
    }
}
