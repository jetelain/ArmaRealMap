using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    /// <summary>
    /// Fill polygons for retail land-use areas not covered by specific OSM features.
    /// Provides the default surface material for retail and shopping zones.
    /// </summary>
    public class DefaultRetailAreaData : DefaultCategoryAreaDataBase
    {
        public DefaultRetailAreaData(List<TerrainPolygon> areas)
            : base(areas) 
        {

        }
    }
}
