using System.Numerics;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Railways
{
    internal class Railway : ITerrainEnvelope, IWay
    {
        public Railway(WaySpecialSegment specialSegment, TerrainPath path)
        {
            SpecialSegment = specialSegment;
            Path = path;
            MinPoint = path.MinPoint - new Vector2(Width);
            MaxPoint = path.MaxPoint + new Vector2(Width);
        }

        public WaySpecialSegment SpecialSegment { get; }

        public TerrainPath Path { get; set; }

        public float Width => 3.290f; // International Union of Railways (UIC) : Kinematic reference profile ( https://en.wikipedia.org/wiki/Loading_gauge )

        public float ClearWidth => 5f; // Width with ~1m margin on each side

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public IEnumerable<TerrainPolygon> ClearPolygons => Path.ToTerrainPolygon(ClearWidth);

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }
    }
}
