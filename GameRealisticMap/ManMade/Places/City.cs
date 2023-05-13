using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Places
{
    public class City
    {
        [JsonConstructor]
        public City(TerrainPoint center, List<TerrainPolygon> boundary, string name, CityTypeId type, float radius, float population)
        {
            Center = center;
            Boundary = boundary;
            Name = name;
            Type = type;
            Radius = radius;
            Population = population;
        }

        public TerrainPoint Center { get; }

        public List<TerrainPolygon> Boundary { get; }

        public string Name { get; }

        public CityTypeId Type { get; }

        public float Radius { get; }

        public float Population { get; }
    }
}
