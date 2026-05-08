using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.Nature;

namespace GameRealisticMap.ManMade
{
    /// <summary>
    /// Contains land-use area polygons classified into categories: residential, commercial,
    /// industrial, military, retail, and agricultural. Derived from OSM landuse tags.
    /// Used as input for default urban-area fill builders and for biome-level rendering decisions.
    /// </summary>
    public class CategoryAreaData : INonDefaultArea
    {
        public static BuildingTypeId[] Categories = new[] {
            BuildingTypeId.Residential,
            BuildingTypeId.Military,
            BuildingTypeId.Commercial,
            BuildingTypeId.Retail,
            BuildingTypeId.Industrial,
            BuildingTypeId.Agricultural };

        public CategoryAreaData(List<CategoryArea> areas)
        {
            Areas = areas;
        }

        public List<CategoryArea> Areas { get; }

        IEnumerable<TerrainPolygon> INonDefaultArea.Polygons => Areas.SelectMany(a => a.PolyList);
    }
}
