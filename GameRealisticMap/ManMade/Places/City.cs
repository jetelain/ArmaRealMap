using System.Numerics;
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

        public static City Square(TerrainPoint center, float size, string name, CityTypeId type = CityTypeId.Village, float population = 0)
        {
            return new City(center, 
                new List<TerrainPolygon>() { TerrainPolygon.FromRectangle(center - new Vector2(size), center + new Vector2(size)) }, 
                name, 
                type, 
                size,
                population);
        }

    }
}
