using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Airports
{
    public sealed class Aeroway
    {
        public Aeroway(TerrainPath segment, AerowayTypeId type, float width, AerowaySurface surface)
        {
            Segment = segment;
            Type = type;
            Width = width;
            OverallVector = Vector2.Normalize(segment.FirstPoint.Vector - segment.LastPoint.Vector);
            Surface = surface;
        }

        public TerrainPath Segment { get; }

        public AerowayTypeId Type { get; }

        public float Width { get; }

        [JsonIgnore]
        public Vector2 OverallVector { get; }

        public AerowaySurface Surface { get; }

        public IEnumerable<TerrainPolygon> ToPolygons(float margin = 0)
        {
            if ( Type == AerowayTypeId.Runway)
            {
                return Segment.ToTerrainPolygonButt(Width + margin);
            }
            return Segment.ToTerrainPolygon(Width + margin);
        }
    }
}
