using System.Diagnostics;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Roads
{
    internal static class RoadTypeIdHelper
    {
        internal static RoadTypeId? FromOSM(TagsCollectionBase tags)
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
                    return RoadTypeId.ConcreteFootway;

                case "pedestrian":
                case "path":
                    var surface = tags.GetValue("surface");
                    if (surface == "asphalt" || surface == "paved")
                    {
                        return RoadTypeId.ConcreteFootway;
                    }
                    return RoadTypeId.Trail;

                case "track":
                    return RoadTypeId.SingleLaneDirtPath;

                case "service":
                    if (tags.GetValue("access") == "private")
                    {
                        // Ignored for optimisation purpose
                        return null;
                    }
                    if (tags.GetValue("service") == "driveway" && tags.GetValue("motor_vehicle") != "permissive" && tags.GetValue("access") != "permit")
                    {
                        // Ignored for optimisation purpose
                        return null;
                    }
                    return RoadTypeId.SingleLaneConcreteRoad;

            }
            return null;
        }

    }
}
