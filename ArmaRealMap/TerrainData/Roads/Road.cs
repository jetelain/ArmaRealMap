using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Roads
{
    public class Road : ITerrainGeometry
    {
        public RoadType RoadType { get; set; }

        public RoadSpecialSegment SpecialSegment { get; set; }

        public TerrainPath Path { get; set; }

        public float Width => RoadTypeToWidth(RoadType);

        public TerrainPoint MinPoint => Path.MinPoint;

        public TerrainPoint MaxPoint => Path.MaxPoint;

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public static float RoadTypeToWidth(RoadType value)
        {
            switch (value)
            {
                case RoadType.TwoLanesMotorway:
                case RoadType.TwoLanesPrimaryRoad:
                    return 12f;
                case RoadType.TwoLanesSecondaryRoad:
                    return 7.5f;
                case RoadType.TwoLanesConcreteRoad:
                    return 7.5f;
                case RoadType.SingleLaneDirtRoad:
                    return 6f;
                case RoadType.SingleLaneDirtPath:
                    return 4.5f;
                case RoadType.Trail:
                    return 1.5f;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value.ToString());
            }
        }
    }
}
