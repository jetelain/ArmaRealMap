using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    /// <summary>
    /// Fill polygons for military land-use areas not covered by specific OSM features.
    /// Provides the default surface material for military zones.
    /// </summary>
    public class DefaultMilitaryAreaData : DefaultCategoryAreaDataBase
    {
        public DefaultMilitaryAreaData(List<TerrainPolygon> areas)
            : base(areas) 
        {

        }
    }
}
