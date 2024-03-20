using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;

namespace GameRealisticMap.ManMade.Airports
{
    public sealed class AirportData : INonDefaultArea, IPolygonTerrainData
    {
        public AirportData(List<TerrainPolygon> polygons)
        {
            Polygons = polygons;
        }

        public List<TerrainPolygon> Polygons { get; }

        IEnumerable<TerrainPolygon> INonDefaultArea.Polygons => Polygons;
    }
}
