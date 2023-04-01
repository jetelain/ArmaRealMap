using GameRealisticMap.Geometries;

namespace GameRealisticMap.Water
{
    internal class WaterData : ITerrainData
    {
        public WaterData(List<TerrainPolygon> lakesPolygons, List<TerrainPath> waterWaysPaths)
        {
            LakesPolygons = lakesPolygons;
            WaterWaysPaths = waterWaysPaths;
        }

        public List<TerrainPolygon> LakesPolygons { get; }

        public List<TerrainPath> WaterWaysPaths { get; }
    }
}
