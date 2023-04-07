using GameRealisticMap.Geometries;
using GeoJSON.Text.Feature;

namespace GameRealisticMap.Nature.WaterWays
{
    public class WaterWaysData : ITerrainData
    {
        public WaterWaysData(List<TerrainPath> waterWaysPaths)
        {
            WaterWaysPaths = waterWaysPaths;
        }

        public List<TerrainPath> WaterWaysPaths { get; }

        public IEnumerable<Feature> ToGeoJson()
        {
            return Enumerable.Empty<Feature>();
        }
    }
}
