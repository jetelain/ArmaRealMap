using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultIndustrialAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultIndustrialAreaData>
    {
        public DefaultIndustrialAreasBuilder(IProgressSystem progress) : base(progress)
        {
        }

        protected override BuildingTypeId TragetedType => BuildingTypeId.Industrial;

        protected override DefaultIndustrialAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultIndustrialAreaData(polygons);
        }
    }
}
