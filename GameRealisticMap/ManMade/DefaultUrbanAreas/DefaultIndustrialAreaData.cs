using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    /// <summary>
    /// Fill polygons for industrial land-use areas not covered by specific OSM features.
    /// Provides the default surface material for industrial zones.
    /// </summary>
    public class DefaultIndustrialAreaData : DefaultCategoryAreaDataBase
    {
        public DefaultIndustrialAreaData(List<TerrainPolygon> areas)
            : base(areas) 
        {

        }
    }
}
