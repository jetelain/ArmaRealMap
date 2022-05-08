using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Core.Roads;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.Roads
{
    public class Road : ITerrainGeometry
    {
        public RoadTypeId RoadType { get; set; }

        public RoadSpecialSegment SpecialSegment { get; set; }

        public TerrainPath Path { get; set; }

        public float Width => RoadTypeInfos.Width;

        public TerrainPoint MinPoint => Path.MinPoint;

        public TerrainPoint MaxPoint => Path.MaxPoint;

        public IEnumerable<TerrainPolygon> Polygons => Path.ToTerrainPolygon(Width);

        public RoadTypeInfos RoadTypeInfos { get; internal set; }

        //public static float RoadTypeToWidth(RoadTypeId value)
        //{
        //    switch (value)
        //    {
        //        case RoadTypeId.TwoLanesMotorway:
        //        case RoadTypeId.TwoLanesPrimaryRoad:
        //            return 12f;
        //        case RoadTypeId.TwoLanesSecondaryRoad:
        //            return 7.5f;
        //        case RoadTypeId.TwoLanesConcreteRoad:
        //            return 7.5f;
        //        case RoadTypeId.SingleLaneDirtRoad:
        //            return 6f;
        //        case RoadTypeId.SingleLaneDirtPath:
        //            return 4.5f;
        //        case RoadTypeId.Trail:
        //            return 1.5f;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(value), value.ToString());
        //    }
        //}
    }
}
