using System.Text.Json.Serialization;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature.Watercourses
{
    public class Watercourse
    {
        [JsonConstructor]
        public Watercourse(TerrainPath path, WatercourseTypeId typeId)
        {
            Path = path;
            TypeId = typeId;
        }

        public TerrainPath Path { get; }

        public WatercourseTypeId TypeId { get; }

        [JsonIgnore]
        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        [JsonIgnore]
        public bool IsTunnel => TypeId >= WatercourseTypeId.RiverTunnel;

        [JsonIgnore]
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
