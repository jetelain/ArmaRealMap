using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMap.Core.Roads;
using GameRealisticMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;

namespace ArmaRealMap.TerrainData.Forests
{
    class NatureBuilder
    {
        public static void PrepareWaterwayEdges(MapData data, List<OsmShape> shapes, ObjectLibraries olibs)
        {
            var lib = olibs.GetSingleLibrary(ObjectCategory.WaterwayBorder);
            if (lib == null)
            {
                return;
            }
            
            var layer = new TerrainObjectLayer(data.MapInfos);

            var waterWays = shapes.Where(s => s.Category == OsmShapeCategory.WaterWay && s.IsPath && !s.OsmGeo.Tags.ContainsKey("tunnel")).ToList();
            var waterWaysPaths = waterWays.SelectMany(w => w.TerrainPaths).ToList();
            var waterWaysPolys = waterWaysPaths.SelectMany(r => r.ToTerrainPolygon(10f)).ToList();

            waterWaysPolys = TerrainPolygon.MergeAll(waterWaysPolys);

            var rnd = new Random(1);
            var report = new ProgressReport("WaterWayEdges", waterWaysPolys.Count);
            foreach (var poly in waterWaysPolys)
            {
                FollowPathWithObjects.PlaceOnEdgeRandomOutside(rnd, lib, layer, poly, 0f);
                report.ReportOneDone();
            }
            report.TaskDone();

            Remove(layer, data.Lakes.Select(l => l.TerrainPolygon), (poly, tree) => poly.ContainsRaw(tree.Center));
            Remove(layer, data.Scrubs, (poly, tree) => poly.ContainsRaw(tree.Center));
            Remove(layer, data.Forests, (poly, tree) => poly.ContainsRaw(tree.Center));

            RemoveOnBuildingsAndRoads(data, layer);

            layer.WriteFile(Path.Combine(data.Config.Target.Objects, "waterway-edge.rel.txt"));
        }

        public static void Prepare(MapData data, List<OsmShape> shapes, ObjectLibraries olibs)
        {
            var forestPolygonsCleaned = GetPolygons(data, shapes, OsmShapeCategory.Forest);

            var objects = new FillShapeWithObjectsClustered(data, ObjectCategory.ForestTree, olibs).Fill(forestPolygonsCleaned, "forest-pass4.txt");
            new FillShapeWithObjectsBasic(data, ObjectCategory.ForestAdditionalObjects, olibs).Fill(objects, forestPolygonsCleaned, "forest-additional.txt"); 

            RemoveOnBuildingsAndRoads(data, objects);

            objects.WriteFile(Path.Combine(data.Config.Target.Objects, "forest.rel.txt"));

            GenerateEdgeObjects(data, olibs, forestPolygonsCleaned);

            GenerateRadialEdgeObjects(data, olibs, shapes, forestPolygonsCleaned, ObjectCategory.ForestRadialEdge, "forest-radial.rel.txt");

            data.Forests = forestPolygonsCleaned;
        }

        private static void GenerateRadialEdgeObjects(MapData data, ObjectLibraries olibs, List<OsmShape> shapes, List<TerrainPolygon> polygons, ObjectCategory category, string filename, float width = 25f)
        {
            var lib = olibs.GetSingleLibrary(category);
            if (lib == null)
            {
                return;
            }
            var radialEdges = CropPolygonsToTerrain(data, polygons.SelectMany(e => e.Crown2(width)));
            var substractPolygons = GetPriorityPolygons(data, shapes, OsmShapeCategory.Dirt);
            var radialCleaned = Subtract(radialEdges, substractPolygons);
            var final = TerrainPolygon.MergeAll(radialCleaned);
            var objects = new FillShapeWithObjectsClustered(data, category, olibs).Fill(final, filename);
            Remove(objects, data.Buildings, (building, tree) => tree.Poly.Distance(building.Poly) < 0.25f);
            Remove(objects, data.Roads, (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= (road.Width / 2) + 1f);
            objects.WriteFile(Path.Combine(data.Config.Target.Objects, filename));
        }

        private static void GenerateEdgeObjects(MapData data, ObjectLibraries olibs,  List<TerrainPolygon> forestPolygonsCleaned)
        {
            var lib = olibs.GetSingleLibrary(ObjectCategory.ForestEdge);
            if ( lib == null)
            {
                return;
            }
            var margin = 1f;
            var edges = forestPolygonsCleaned.SelectMany(f => f.Offset(-margin)).ToList();
            var edgeObjects = new TerrainObjectLayer(data.MapInfos);
            var rnd = new Random(1);
            var report = new ProgressReport("EdgeObjects", edges.Count);
            foreach (var edgePoly in edges)
            {
                FollowPathWithObjects.PlaceOnEdgeRandomInside(rnd, lib, edgeObjects, edgePoly, margin);
                report.ReportOneDone();
            }
            report.TaskDone();
            Remove(edgeObjects, data.Roads, (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= road.Width / 2);
            edgeObjects.WriteFile(Path.Combine(data.Config.Target.Objects, "forest-edge.rel.txt"));
        }

        private static void RemoveOverlaps(List<TerrainPolygon> forest)
        {
            var report = new ProgressReport("RemoveOverlaps", forest.Count);
            var initialCount = forest.Count;
            foreach (var item in forest.ToList())
            {
                if (forest.Contains(item))
                {
                    var contains = forest.Where(f => f != item && item.ContainsOrSimilar(f)).ToList();
                    if (contains.Count > 0)
                    {
                        foreach (var toremove in contains)
                        {
                            forest.Remove(toremove);
                        }
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            Console.WriteLine("Removed {0} overlap areas", initialCount - forest.Count);
        }

        private static List<TerrainPolygon> Subtract(List<TerrainPolygon> forestPolygonsCropped, List<TerrainPolygon> substractPolygons)
        {
            var forestPolygonsCleaned = new List<TerrainPolygon>();
            var report2 = new ProgressReport("Subtract", forestPolygonsCropped.Count);
            foreach (var shape in forestPolygonsCropped)
            {
                if (shape.EnveloppeSize.X > 100 || shape.EnveloppeSize.Y > 100)
                {
                    foreach (var remain in shape.SubstractAll(substractPolygons))
                    {
                        forestPolygonsCleaned.Add(remain);
                    }
                }
                else
                {
                    forestPolygonsCleaned.Add(shape); // leave alone really small areas
                }
                report2.ReportOneDone();
            }
            report2.TaskDone();
            return forestPolygonsCleaned;
        }

        private static List<TerrainPolygon> GetPriorityPolygons(MapData data, List<OsmShape> shapes, OsmShapeCategory category)
        {
            var substractPolygons = new List<TerrainPolygon>();

            // Buildings (real ones)
            substractPolygons.AddRange(data.Buildings
                .SelectMany(b => TerrainPolygon.FromPolygon(b.Poly, c => new TerrainPoint((float)c.X, (float)c.Y)))
                .SelectMany(b => b.Offset(0.25f))); // Little margin

            // Roads that have an edge effect of forest
            substractPolygons.AddRange(data.Roads
                .Where(r => r.RoadType != RoadTypeId.Trail)
                .SelectMany(r => TerrainPolygon.FromPath(r.Path.Points, RoadClearWidth(r))));

            // Priority shapes
            substractPolygons.AddRange(shapes
                .Where(s => s.Category.GroundTexturePriority < category.GroundTexturePriority
                    && !s.Category.IsBuilding
                    && s.Category != OsmShapeCategory.Building
                    && s.Category != OsmShapeCategory.Water
                    && s.Category != OsmShapeCategory.WaterWay)
                .SelectMany(g => g.TerrainPolygons));

            return substractPolygons;
        }

        private static List<TerrainPolygon> GetCroppedPolygons(MapData data, List<OsmShape> shapes, OsmShapeCategory category)
        {
            return CropPolygonsToTerrain(data, shapes.Where(s => s.Category == category).SelectMany(s => s.TerrainPolygons));
        }

        public static List<TerrainPolygon> CropPolygonsToTerrain(MapData data, IEnumerable<TerrainPolygon> forestPolygons)
        {
            var list = forestPolygons.ToList();
            var area = data.MapInfos.Polygon;
            var polygonsCropped = new List<TerrainPolygon>();
            var report = new ProgressReport("Crop", list.Count);
            foreach (var poly in list)
            {
                foreach (var cropped in poly.ClippedBy(area))
                {
                    polygonsCropped.Add(cropped);
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            return polygonsCropped;
        }

        private static float RoadClearWidth(Road r)
        {
            if (r.RoadType < RoadTypeId.TwoLanesPrimaryRoad)
            {
                return r.Width + 6f;
            }
            if (r.RoadType < RoadTypeId.SingleLaneDirtPath)
            {
                return r.Width + 4f;
            }
            return r.Width + 2f;
        }

        public static void Remove<T>(TerrainObjectLayer objects, IEnumerable<T> toremoveList, Func<T, TerrainObject, bool> match)
            where T : ITerrainGeometry
        {
            var list = toremoveList.ToList();

            var report = new ProgressReport("Remove", list.Count);
            var removed = 0;
            foreach (var toremove in list)
            {
                foreach (var obj in objects.Search(toremove.MinPoint, toremove.MaxPoint))
                {
                    if (match(toremove, obj))
                    {
                        objects.Remove(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
                        removed++;
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        public static void PrepareScrub(MapData data, List<OsmShape> shapes, ObjectLibraries olibs)
        {
            var polygonsCleaned = GetPolygons(data, shapes, OsmShapeCategory.Scrub);

            var objects = new FillShapeWithObjectsClustered(data, ObjectCategory.Scrub, olibs).Fill(polygonsCleaned, "scrub-pass4.txt");
            new FillShapeWithObjectsBasic(data, ObjectCategory.ScrubAdditionalObjects, olibs).Fill(objects, polygonsCleaned, "scrub-additional.txt");

            RemoveOnBuildingsAndRoads(data, objects);

            objects.WriteFile(Path.Combine(data.Config.Target.Objects, "scrub.rel.txt"));

            data.Scrubs = polygonsCleaned;

            GenerateRadialEdgeObjects(data, olibs, shapes, polygonsCleaned, ObjectCategory.ScrubRadialEdge, "scrub-radial.rel.txt");
        }

        public static void PrepareGroundRock(MapData data, List<OsmShape> shapes, ObjectLibraries olibs)
        {
            var polygonsCleaned = GetPolygons(data, shapes, OsmShapeCategory.Rocks);

            var objects = new FillShapeWithObjectsBasic(data, ObjectCategory.GroundRock, olibs) { MustFullFit = true }.Fill(polygonsCleaned, "rocks-pass4.txt");
            new FillShapeWithObjectsBasic(data, ObjectCategory.GroundRockAdditionalObjects, olibs).Fill(objects, polygonsCleaned, "rocks-additional.txt");

            RemoveOnBuildingsAndRoads(data, objects);

            objects.WriteFile(Path.Combine(data.Config.Target.Objects, "rocks.rel.txt"));
        }

        internal static void RemoveOnBuildingsAndRoads(MapData data, TerrainObjectLayer objects)
        {
            Remove(objects, data.Buildings, (building, tree) => tree.Poly.Distance(building.Poly) < 0.25f);
            Remove(objects, data.Roads, (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= (road.Width / 2) + 1f);
        }

        private static List<TerrainPolygon> GetPolygons(MapData data, List<OsmShape> shapes, OsmShapeCategory category)
        {
            var polygonsCropped = GetCroppedPolygons(data, shapes, category);
            RemoveOverlaps(polygonsCropped);
            return Subtract(polygonsCropped, GetPriorityPolygons(data, shapes, category));
        }
    }
}
