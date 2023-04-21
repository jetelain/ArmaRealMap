using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature.Watercourses
{
    public class Watercourse
    {
        public Watercourse(TerrainPath path, WatercourseTypeId id)
        {
            Path = path;
            TypeId = id;
        }

        public TerrainPath Path { get; }

        public WatercourseTypeId TypeId { get; }

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public bool IsTunnel => TypeId >= WatercourseTypeId.RiverTunnel;

        public float Width // Create a library for that ?
        {
            get
            {
                switch (TypeId)
                {
                    case WatercourseTypeId.Stream:
                        return 4;

                    case WatercourseTypeId.River:
                        return 10;
                }
                return 0;
            }
        }
    }
}
