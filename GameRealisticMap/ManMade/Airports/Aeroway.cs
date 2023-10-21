using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Airports
{
    public class Aeroway
    {
        public Aeroway(TerrainPath segment, AerowayTypeId type, float width)
        {
            Segment = segment;
            Type = type;
            Width = width;
        }

        public TerrainPath Segment { get; }

        public AerowayTypeId Type { get; }

        public float Width { get; }
    }
}
