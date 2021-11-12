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

        public static float RoadTypeToWidth(RoadType value)
        {
            switch (value)
            {
                case RoadType.TwoLanesMotorway:
                    return 10f;
                case RoadType.TwoLanesPrimaryRoad:
                    return 8f;
                case RoadType.TwoLanesSecondaryRoad:
                case RoadType.TwoLanesCityRoad:
                case RoadType.TwoLanesConcreteRoad:
                    return 7f;
                case RoadType.SingleLaneDirtRoad:
                case RoadType.SingleLaneDirtPath:
                    return 3.5f;
                default:
                case RoadType.Trail:
                    return 1f;
            }
        }
    }
}
