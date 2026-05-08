using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    /// <summary>
    /// Fill polygons for agricultural land-use areas not covered by specific OSM features.
    /// Provides the default surface material for general agricultural zones.
    /// </summary>
    public class DefaultAgriculturalAreaData : DefaultCategoryAreaDataBase
    {
        public DefaultAgriculturalAreaData(List<TerrainPolygon> areas)
            : base(areas) 
        {

        }
    }
}
