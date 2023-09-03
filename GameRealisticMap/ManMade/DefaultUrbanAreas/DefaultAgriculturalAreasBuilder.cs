using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultAgriculturalAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultAgriculturalAreaData>
    {
        public DefaultAgriculturalAreasBuilder(IProgressSystem progress) : base(progress)
        {
        }

        protected override BuildingTypeId TragetedType => BuildingTypeId.Agricultural;

        protected override DefaultAgriculturalAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultAgriculturalAreaData(polygons);
        }
    }
}
