using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature
{
    internal class ScrubData : IBasicTerrainData
    {
        public ScrubData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }
    }
}
