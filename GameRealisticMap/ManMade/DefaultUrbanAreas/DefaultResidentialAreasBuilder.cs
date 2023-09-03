using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultResidentialAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultResidentialAreaData>
    {
        public DefaultResidentialAreasBuilder(IProgressSystem progress) : base(progress)
        {
        }

        protected override BuildingTypeId TragetedType => BuildingTypeId.Residential;

        protected override DefaultResidentialAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultResidentialAreaData(polygons);
        }
    }
}
