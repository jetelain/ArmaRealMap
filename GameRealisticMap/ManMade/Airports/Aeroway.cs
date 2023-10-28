using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Airports
{
    public sealed class Aeroway
    {
        public Aeroway(TerrainPath segment, AerowayTypeId type, float width)
        {
            Segment = segment;
            Type = type;
            Width = width;
            OverallVector = Vector2.Normalize(segment.FirstPoint.Vector - segment.LastPoint.Vector);
        }

        public TerrainPath Segment { get; }

        public AerowayTypeId Type { get; }

        public float Width { get; }

        [JsonIgnore]
        public Vector2 OverallVector { get; }
    }
}
