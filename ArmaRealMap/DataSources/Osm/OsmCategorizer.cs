using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ArmaRealMap.Roads;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Geo;
using OsmSharp.Streams;
using OsmSharp.Tags;

namespace ArmaRealMap.Osm
{
    internal static class OsmCategorizer
    {

        internal static List<OsmShape> GetShapes(SnapshotDb db, OsmStreamSource filtered, MapInfos mapInfos)
        {
            Console.WriteLine("Filter OSM data...");
            var toRender = new List<OsmShape>();
            var interpret = new DefaultFeatureInterpreter2();
            var list = filtered.Where(osmGeo =>
            (osmGeo.Type == OsmSharp.OsmGeoType.Way || osmGeo.Type == OsmSharp.OsmGeoType.Relation)
            && osmGeo.Tags != null).ToList();
            var report = new ProgressReport("GetShapes", list.Count);
            foreach (OsmGeo osmGeo in list)
            {
                var category = GetCategory(osmGeo.Tags, interpret);
                if (category != null)
                {
                    var complete = osmGeo.CreateComplete(db);
                    var count = 0;
                    foreach (var feature in interpret.Interpret(complete))
                    {
                        toRender.Add(new OsmShape(category, osmGeo, feature.Geometry, mapInfos));
                        count++;
                    }
                    if (count == 0)
                    {
                        Trace.TraceWarning($"NO GEOMETRY FOR {osmGeo.Tags}");
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            return toRender;
        }

        private static OsmShapeCategory GetCategory(TagsCollectionBase tags, FeatureInterpreter interpreter)
        {
            if ((tags.ContainsKey("waterway") && !tags.IsFalse("waterway")))
            {
                return OsmShapeCategory.WaterWay;
            }
            if (tags.ContainsKey("water"))
            {
                if (Get(tags, "water") == "lake")
                {
                    return OsmShapeCategory.Lake;
                }
                if (Get(tags, "water") == "stream")
                {
                    return OsmShapeCategory.WaterWay;
                }
                return OsmShapeCategory.Water;
            }
            if (tags.ContainsKey("building") && !tags.IsFalse("building"))
            {
                switch (Get(tags, "building"))
                {
                    case "church":
                        return OsmShapeCategory.BuildingChurch;
                    case "hut":
                        return OsmShapeCategory.BuildingHut;
                }
                if (Get(tags, "historic") == "fort")
                {
                    return OsmShapeCategory.BuildingHistoricalFort;
                }
                if (Get(tags, "tower:type") == "communication")
                {
                    return OsmShapeCategory.BuildingRadioTower;
                }
                if (tags.ContainsKey("brand"))
                {
                    return OsmShapeCategory.BuildingRetail;
                }
                return OsmShapeCategory.Building;
            }

            if (Get(tags, "type") == "boundary")
            {
                return null;
            }

            switch (Get(tags, "surface"))
            {
                case "grass": return OsmShapeCategory.Grass;
                case "sand": return OsmShapeCategory.Sand;
                case "concrete": return OsmShapeCategory.Concrete;
            }



            switch (Get(tags, "landuse"))
            {
                case "forest": return OsmShapeCategory.Forest;
                case "grass": return OsmShapeCategory.Grass;
                case "village_green": return OsmShapeCategory.Grass;
                case "farmland": return OsmShapeCategory.FarmLand;
                case "farmyard": return OsmShapeCategory.FarmLand;
                case "vineyard": return OsmShapeCategory.FarmLand;
                case "orchard": return OsmShapeCategory.FarmLand;
                case "meadow": return OsmShapeCategory.FarmLand;
                case "industrial": return OsmShapeCategory.Industrial;
                case "commercial": return OsmShapeCategory.Commercial;
                case "residential": return OsmShapeCategory.Residential;
                case "cemetery": return OsmShapeCategory.Grass;
                case "railway": return OsmShapeCategory.Dirt;
                case "retail": return OsmShapeCategory.Retail;
                case "basin": return OsmShapeCategory.Water;
                case "reservoir": return OsmShapeCategory.Lake;
                case "allotments": return OsmShapeCategory.Grass;
                case "military": return OsmShapeCategory.Military;
            }

            switch (Get(tags, "natural"))
            {
                case "wood": return OsmShapeCategory.Forest;
                case "water": return OsmShapeCategory.Lake;
                case "grass": return OsmShapeCategory.Grass;
                case "heath": return OsmShapeCategory.Grass;
                case "meadow": return OsmShapeCategory.Grass;
                case "grassland": return OsmShapeCategory.Grass;
                case "scrub": return OsmShapeCategory.Scrub;
                case "wetland": return OsmShapeCategory.WetLand;
                case "tree_row": return OsmShapeCategory.Forest;
                case "scree": return OsmShapeCategory.Sand;
                case "sand": return OsmShapeCategory.Sand;
                case "beach": return OsmShapeCategory.Sand;
                case "bare_rock": return OsmShapeCategory.Rocks;
            }

            if (Get(tags, "leisure") == "garden")
            {
                return OsmShapeCategory.Grass;
            }

            var road = ToRoadType(tags);
            if (road != null && road.Value < RoadType.SingleLaneDirtRoad)
            {
                return OsmShapeCategory.Road;
            }

            if (interpreter.IsPotentiallyArea(tags))
            {
                tags.RemoveKey("source");
                tags.RemoveKey("name");
                tags.RemoveKey("alt_name");
                Trace.WriteLine(tags);
                //Console.WriteLine(tags);
            }
            return null;
        }

        internal static RoadType? ToRoadType(TagsCollectionBase tags)
        {
            var type = tags.GetValue("highway");
            switch (type)
            {
                case "motorway":
                    return RoadType.TwoLanesMotorway;
                case "trunk":
                case "primary":
                case "primary_link":
                case "trunk_link":
                case "motorway_link":
                    return RoadType.TwoLanesPrimaryRoad;
                case "secondary":
                case "tertiary":
                case "seconday_link":
                case "tertiary_link":
                case "road":
                    return RoadType.TwoLanesSecondaryRoad;
                case "living_street":
                case "residential":
                case "unclassified":
                    return RoadType.TwoLanesConcreteRoad;
                case "footway":
                    var footway = Get(tags, "footway");
                    if (footway == "sidewalk" || footway == "crossing")
                    {
                        return null;
                    }
                    var side = Get(tags, "sidewalk");
                    if (!string.IsNullOrEmpty(side) && side != "no")
                    {
                        return null;
                    }
                    return RoadType.Trail;
                case "pedestrian":
                case "path":
                    return RoadType.Trail;
                case "track":
                    return RoadType.SingleLaneDirtPath;
            }
            if ( !string.IsNullOrEmpty(type))
            {
                Trace.WriteLine($"Unknown highway='{type}'");
            }
            return null;
        }

        internal static string Get(TagsCollectionBase tags, string key)
        {
            string value;
            if (tags != null && tags.TryGetValue(key, out value))
            {
                return value;
            }
            return null;
        }

        internal static RoadSpecialSegment ToRoadSpecialSegment(TagsCollectionBase tags)
        {
            if (Get(tags, "embankment") == "yes")
            {
                return RoadSpecialSegment.Embankment;
            }
            if (Get(tags, "bridge") == "yes")
            {
                return RoadSpecialSegment.Bridge;
            }
            return RoadSpecialSegment.Normal;
        }
    }
}
