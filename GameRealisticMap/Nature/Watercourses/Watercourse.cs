using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature.Watercourses
{
    public class Watercourse
    {
        public Watercourse(TerrainPath path, WatercourseId id)
        {
            Path = path;
            TypeId = id;
        }

        public TerrainPath Path { get; }

        public WatercourseId TypeId { get; }

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public bool IsTunnel => TypeId >= WatercourseId.RiverTunnel;

        public float Width // Create a library for that
        {
            get
            {
                switch (TypeId)
                {
                    case WatercourseId.Stream:
                        return 4;

                    case WatercourseId.River:
                        return 10;
                }
                return 0;
            }
        }
    }
}
