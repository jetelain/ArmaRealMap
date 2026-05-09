using GameRealisticMap.Geometries;
using GameRealisticMap.Nature;

namespace GameRealisticMap.ManMade.Airports
{
    /// <summary>
    /// Contains airport and aerodrome boundary polygons extracted from OSM
    /// (aeroway=aerodrome). Treated as a non-default area that suppresses generic surface fill.
    /// </summary>
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
