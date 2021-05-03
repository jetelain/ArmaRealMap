using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Geo;
using OsmSharp.Streams;
using OsmSharp.Tags;

namespace ArmaRealMap
{
    internal static class OsmCategorizer
    {

        internal static List<Area> GetShapes(SnapshotDb db, OsmStreamSource filtered)
        {
            Console.WriteLine("Filter OSM data...");
            var toRender = new List<Area>();
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
                        toRender.Add(new Area(category, osmGeo, feature.Geometry));
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

        private static AreaCategory GetCategory(TagsCollectionBase tags, FeatureInterpreter interpreter)
        {
            if (tags.ContainsKey("water") || (tags.ContainsKey("waterway") && !tags.IsFalse("waterway")))
            {
                return AreaCategory.Water;
            }
            if (tags.ContainsKey("building") && !tags.IsFalse("building"))
            {
                switch (Get(tags, "building"))
                {
                    case "church":
                        return AreaCategory.BuildingChurch;
                }
                if (Get(tags, "historic") == "fort")
                {
                    return AreaCategory.BuildingHistoricalFort;
                }
                if (tags.ContainsKey("brand"))
                {
                    return AreaCategory.BuildingRetail;
                }
                return AreaCategory.Building;
            }

            if (Get(tags, "type") == "boundary")
            {
                return null;
            }

            switch (Get(tags, "surface"))
            {
                case "grass": return AreaCategory.Grass;
                case "sand": return AreaCategory.Sand;
                case "concrete": return AreaCategory.Concrete;
            }



            switch (Get(tags, "landuse"))
            {
                case "forest": return AreaCategory.Forest;
                case "grass": return AreaCategory.Grass;
                case "village_green": return AreaCategory.Grass;
                case "farmland": return AreaCategory.FarmLand;
                case "farmyard": return AreaCategory.FarmLand;
                case "vineyard": return AreaCategory.FarmLand;
                case "orchard": return AreaCategory.FarmLand;
                case "meadow": return AreaCategory.FarmLand;
                case "industrial": return AreaCategory.Industrial;
                case "residential": return AreaCategory.Residential;
                case "cemetery": return AreaCategory.Grass;
                case "railway": return AreaCategory.Dirt;
                case "retail": return AreaCategory.Retail;
                case "basin": return AreaCategory.Water;
                case "reservoir": return AreaCategory.Water;
                case "allotments": return AreaCategory.Grass;
                case "military": return AreaCategory.Military;
            }

            switch (Get(tags, "natural"))
            {
                case "wood": return AreaCategory.Forest;
                case "water": return AreaCategory.Water;
                case "grass": return AreaCategory.Grass;
                case "heath": return AreaCategory.Grass;
                case "meadow": return AreaCategory.Grass;
                case "grassland": return AreaCategory.Grass;
                case "scrub": return AreaCategory.Grass;
                case "wetland": return AreaCategory.WetLand;
                case "tree_row": return AreaCategory.Forest;
                case "scree": return AreaCategory.Sand;
                case "sand": return AreaCategory.Sand;
                case "beach": return AreaCategory.Sand;
            }

            if (Get(tags, "leisure") == "garden")
            {
                return AreaCategory.Grass;
            }

            var road = ToRoadType(Get(tags, "highway"));
            if (road != null && road.Value < RoadType.SingleLaneDirtRoad)
            {
                return AreaCategory.Road;
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

        internal static RoadType? ToRoadType(string highway)
        {
            switch (highway)
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
                case "unclassified":
                case "road":
                    return RoadType.TwoLanesSecondaryRoad;
                case "living_street":
                case "residential":
                case "pedestrian":
                    return RoadType.TwoLanesConcreteRoad;
                case "footway":
                    return RoadType.SingleLaneDirtRoad;
                case "path":
                    return RoadType.Trail;
                case "track":
                    return RoadType.SingleLaneDirtPath;
            }
            Trace.WriteLine($"Unknown highway='{highway}'");
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
    }
}
