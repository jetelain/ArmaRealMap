using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    /// <summary>
    /// Fill polygons for commercial land-use areas not covered by specific OSM features.
    /// Provides the default surface material for commercial zones.
    /// </summary>
    public class DefaultCommercialAreaData : DefaultCategoryAreaDataBase
    {
        public DefaultCommercialAreaData(List<TerrainPolygon> areas)
            : base(areas) 
        {

        }
    }
}
