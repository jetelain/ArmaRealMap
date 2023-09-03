using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultRetailAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultRetailAreaData>
    {
        public DefaultRetailAreasBuilder(IProgressSystem progress) : base(progress)
        {
        }

        protected override BuildingTypeId TragetedType => BuildingTypeId.Retail;

        protected override DefaultRetailAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultRetailAreaData(polygons);
        }
    }
}
