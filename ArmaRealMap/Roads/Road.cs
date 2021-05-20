using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Roads
{
    public class Road
    {
        public RoadType RoadType { get; set; }

        public TerrainPath Path { get; set; }

        public float Width => RoadTypeToWidth(RoadType);

        public static float RoadTypeToWidth(RoadType value)
        {
            switch (value)
            {
                case RoadType.TwoLanesMotorway:
                    return 8f;
                case RoadType.TwoLanesPrimaryRoad:
                    return 7f;
                case RoadType.TwoLanesSecondaryRoad:
                case RoadType.TwoLanesCityRoad:
                case RoadType.TwoLanesConcreteRoad:
                    return 6f;
                case RoadType.SingleLaneDirtRoad:
                    return 1.5f;
                case RoadType.SingleLaneDirtPath:
                    return 1f;
                default:
                case RoadType.Trail:
                    return 0.5f;
            }
        }
    }
}
