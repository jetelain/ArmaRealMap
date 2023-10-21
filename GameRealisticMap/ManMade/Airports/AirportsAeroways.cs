using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Airports
{
    public class AirportsAeroways
    {
        public AirportsAeroways(TerrainPolygon polygon)
        {
            Polygon = polygon;
        }

        public List<Aeroway> Aeroways { get; } = new List<Aeroway>();

        public TerrainPolygon Polygon { get; }
    }
}