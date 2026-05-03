using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.DefaultUrbanAreas
{
    /// <summary>
    /// Fill polygons for residential land-use areas not covered by specific OSM features
    /// (buildings, roads, etc.). Provides the default surface material for residential zones.
    /// </summary>
    public class DefaultResidentialAreaData : DefaultCategoryAreaDataBase
    {
        public DefaultResidentialAreaData(List<TerrainPolygon> areas)
            : base(areas) 
        {

        }
    }
}
