using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    internal class ForestData : IBasicTerrainData
    {
        public ForestData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
    }
}
