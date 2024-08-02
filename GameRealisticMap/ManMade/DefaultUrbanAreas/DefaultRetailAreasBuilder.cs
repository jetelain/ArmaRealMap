using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    internal class DefaultRetailAreasBuilder : DefaultUrbanAreasBuilderBase<DefaultRetailAreaData>
    {
        protected override BuildingTypeId TragetedType => BuildingTypeId.Retail;

        protected override DefaultRetailAreaData Create(List<TerrainPolygon> polygons)
        {
            return new DefaultRetailAreaData(polygons);
        }
    }
}
