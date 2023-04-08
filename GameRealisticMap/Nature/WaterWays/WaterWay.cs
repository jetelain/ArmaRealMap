using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature.WaterWays
{
    public class WaterWay
    {
        public WaterWay(TerrainPath path, WaterWayId id)
        {
            Path = path;
            TypeId = id;
        }

        public TerrainPath Path { get; }

        public WaterWayId TypeId { get; }

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public bool IsTunnel => TypeId >= WaterWayId.RiverTunnel;

        public float Width // Create a library for that
        {
            get
            {
                switch (TypeId)
                {
                    case WaterWayId.Stream:
                        return 4;

                    case WaterWayId.River:
                        return 10;
                }
                return 0;
            }
        }
    }
}
