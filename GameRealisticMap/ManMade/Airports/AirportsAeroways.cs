using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Airports
{
    public sealed class AirportsAeroways
    {
        public AirportsAeroways(TerrainPolygon polygon, List<Aeroway> aeroways)
        {
            Aeroways = aeroways;
            Polygon = polygon;
        }

        public List<Aeroway> Aeroways { get; }

        public TerrainPolygon Polygon { get; }
    }
}