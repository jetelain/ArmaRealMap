using GameRealisticMap.Geometries;

namespace GameRealisticMap.ManMade.Fences
{
    public class Fence
    {
        public Fence(TerrainPath path, FenceTypeId id)
        {
            Path = path;
            TypeId = id;
        }

        public TerrainPath Path { get; }

        public FenceTypeId TypeId { get; }

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public float Width // Create a library for that ?
        {
            get
            {
                if (TypeId == FenceTypeId.Hedge)
                {
                    return 0.5f;
                }
                return 0.2f;
            }
        }
    }
}
