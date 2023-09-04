using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultMilitaryAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultMilitaryAreaData>
    {
        public DefaultMilitaryAreasBuilder(IProgressSystem progress) : base(progress)
        {
        }

        protected override BuildingTypeId TragetedType => BuildingTypeId.Military;

        protected override DefaultMilitaryAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultMilitaryAreaData(polygons);
        }
    }
}
