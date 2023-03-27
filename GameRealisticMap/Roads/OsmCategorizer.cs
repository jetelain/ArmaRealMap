using System.Diagnostics;
using GameRealisticMap.Roads;
using OsmSharp.Tags;

namespace GameRealisticMap.Osm
{
    internal static class OsmRoadCategorizer
    {
        internal static RoadTypeId? ToRoadType(TagsCollectionBase tags)
        {
            var type = tags.GetValue("highway");
            switch (type)
            {
                case "motorway":
                    return RoadTypeId.TwoLanesMotorway;
                case "trunk":
                case "primary":
                case "primary_link":
                case "trunk_link":
                case "motorway_link":
                    return RoadTypeId.TwoLanesPrimaryRoad;
                case "secondary":
                case "tertiary":
                case "seconday_link":
                case "tertiary_link":
                case "road":
                    return RoadTypeId.TwoLanesSecondaryRoad;
                case "living_street":
                case "residential":
                case "unclassified":
                    return RoadTypeId.TwoLanesConcreteRoad;
                case "footway":
                    var footway = tags.GetValue("footway");
                    if (footway == "sidewalk" || footway == "crossing")
                    {
                        return null;
                    }
                    var side = tags.GetValue("sidewalk");
                    if (!string.IsNullOrEmpty(side) && side != "no")
                    {
                        return null;
                    }
                    return RoadTypeId.Trail;
                case "pedestrian":
                case "path":
                    return RoadTypeId.Trail;
                case "track":
                    return RoadTypeId.SingleLaneDirtPath;
            }
            if (!string.IsNullOrEmpty(type))
            {
                Trace.WriteLine($"Unknown highway='{type}'");
            }
            return null;
        }

        internal static RoadSpecialSegment ToRoadSpecialSegment(TagsCollectionBase tags)
        {
            if (tags.GetValue("embankment") == "yes")
            {
                return RoadSpecialSegment.Embankment;
            }
            if (tags.GetValue("bridge") == "yes")
            {
                return RoadSpecialSegment.Bridge;
            }
            return RoadSpecialSegment.Normal;
        }
    }
}
