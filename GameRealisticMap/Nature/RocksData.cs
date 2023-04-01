using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    internal class RocksData : IBasicTerrainData
    {
        public RocksData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
    }
}
