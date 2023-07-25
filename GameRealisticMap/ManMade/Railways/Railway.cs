using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Railways
{
    public class Railway : ITerrainEnvelope, IWay
    {
        [JsonConstructor]
        public Railway(WaySpecialSegment specialSegment, TerrainPath path)
        {
            SpecialSegment = specialSegment;
            Path = path;
            MinPoint = path.MinPoint - new Vector2(Width);
            MaxPoint = path.MaxPoint + new Vector2(Width);
        }

        public WaySpecialSegment SpecialSegment { get; }

        public TerrainPath Path { get; set; }

        [JsonIgnore]
        public float Width => 3.290f; // International Union of Railways (UIC) : Kinematic reference profile ( https://en.wikipedia.org/wiki/Loading_gauge )

        [JsonIgnore]
        public float ClearWidth => 5f; // Width with ~1m margin on each side

        [JsonIgnore]
        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        [JsonIgnore]
        public IEnumerable<TerrainPolygon> ClearPolygons => Path.ToTerrainPolygon(ClearWidth);

        [JsonIgnore]
        public TerrainPoint MinPoint { get; }

        [JsonIgnore]
        public TerrainPoint MaxPoint { get; }
    }
}
