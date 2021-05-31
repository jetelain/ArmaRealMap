using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;

namespace ArmaRealMap.TerrainData.Forests
{
    class ForestBuilder
    {

        public static void Prepare(MapData data, List<OsmShape> shapes, ObjectLibraries olibs)
        {
            var forestPolygonsCropped = GetCroppedForestPolygons(data, shapes);

            //DebugHelper.Polygons(data, forestPolygonsCropped, "forest-pass1.bmp");

            RemoveOverlaps(forestPolygonsCropped);

            //DebugHelper.Polygons(data, forestPolygonsCropped, "forest-pass2.bmp");

            var substractPolygons = GetPriorityPolygons(data, shapes);

            var forestPolygonsCleaned = GetForestPolygons(forestPolygonsCropped, substractPolygons);

            //DebugHelper.Polygons(data, forestPolygonsCleaned, "forest-pass3.bmp");

            var objects = new FillShapeWithObjects(data, olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.ForestTree))
                    .Fill(forestPolygonsCleaned, 0.0100f, "forest-pass4.txt");

            DebugHelper.ObjectsInPolygons(data, objects, forestPolygonsCleaned, "forest-pass4.bmp");

            // Remove trees at edge that may be a problem

            // Too close of building
            Remove(objects, data.Buildings, (building, tree) => tree.Poly.Distance(building.Poly) < 0.25f);

            // Too close of roads
            Remove(objects, data.Roads, (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= (road.Width / 2) + 1f);

            objects.WriteFile(data.Config.Target.GetTerrain("forest.txt"));
            DebugHelper.ObjectsInPolygons(data, objects, forestPolygonsCleaned, "forest-pass5.bmp");

            GenerateEdgeObjects(data, olibs, forestPolygonsCleaned);

            DebugHelper.ObjectsInPolygons(data, wasRemoved, forestPolygonsCleaned, "forest-wasRemoved.bmp");

            data.Forests = forestPolygonsCleaned;

        }

        private static void GenerateEdgeObjects(MapData data, ObjectLibraries olibs, List<TerrainPolygon> forestPolygonsCleaned)
        {
            var margin = 1f;

            var edges = forestPolygonsCleaned.SelectMany(f => f.Offset(-margin)).ToList();

            /*var edgeObjects = new FillShapeWithObjects(data, olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.ForestEdge))
                    .Fill(edges, 0.1f, "forest-edge.txt");*/

            var lib = olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.ForestEdge);

            var edgeObjects = new TerrainObjectLayer(data.MapInfos);
            var rotate = Matrix3x2.CreateRotation(1.570796f);
            var rnd = new Random(1);
            var report = new ProgressReport("EdgeObjects", edges.Count);
            foreach (var edgePoly in edges)
            {
                foreach (var ring in edgePoly.Holes.Concat(new[] { edgePoly.Shell }))
                {
                    var previous = ring.First();
                    var result = new List<TerrainPoint>() { previous };
                    var previousObj = lib.Objects[rnd.Next(0, lib.Objects.Count - 1)];
                    var remainLength = previousObj.GetPlacementRadius();
                    edgeObjects.Insert(new TerrainObject(previousObj, previous, (float)rnd.NextDouble() * 360));
                    foreach (var point in ring.Skip(1))
                    {
                        var delta = point.Vector - previous.Vector;
                        var length = delta.Length();
                        var normalDelta = Vector2.Transform(Vector2.Normalize(delta), rotate);
                        float positionOnSegment = remainLength;
                        while (positionOnSegment <= length)
                        {
                            var obj = lib.Objects[rnd.Next(0, lib.Objects.Count - 1)];
                            var objPoint = new TerrainPoint(Vector2.Lerp(previous.Vector, point.Vector, positionOnSegment / length));
                            if (obj.GetPlacementRadius() > margin)
                            {
                                var dist = (obj.GetPlacementRadius() - margin);
                                objPoint = objPoint + (normalDelta * (dist + obj.GetPlacementRadius() * (float)rnd.NextDouble()));
                            }
                            if (data.MapInfos.IsInside(objPoint))
                            {
                                edgeObjects.Insert(new TerrainObject(obj, objPoint, (float)rnd.NextDouble() * 360));
                            }
                            var minimalDelta = (obj.GetPlacementRadius() + previousObj.GetPlacementRadius());
                            positionOnSegment += minimalDelta + (float)(rnd.NextDouble() * minimalDelta / 2); // next position with up to 50% space
                            previousObj = obj;
                        }
                        remainLength = positionOnSegment - length;
                        previous = point;
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            Remove(edgeObjects, data.Roads, (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= road.Width / 2);

            edgeObjects.WriteFile(data.Config.Target.GetTerrain("forest-edge.txt"));

            DebugHelper.ObjectsInPolygons(data, edgeObjects, forestPolygonsCleaned, "forest-edges.bmp");
        }

        private static void RemoveOverlaps(List<TerrainPolygon> forest)
        {
            var report = new ProgressReport("RemoveOverlaps", forest.Count);
            var initialCount = forest.Count;
            foreach (var item in forest.ToList())
            {
                if (forest.Contains(item))
                {
                    var contains = forest.Where(f => f != item && item.Contains(f)).ToList();
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

        private static List<TerrainPolygon> GetForestPolygons(List<TerrainPolygon> forestPolygonsCropped, List<TerrainPolygon> substractPolygons)
        {
            var forestPolygonsCleaned = new List<TerrainPolygon>();
            var report2 = new ProgressReport("ForestOverlap", forestPolygonsCropped.Count);
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

        private static List<TerrainPolygon> GetPriorityPolygons(MapData data, List<OsmShape> shapes)
        {
            var substractPolygons = new List<TerrainPolygon>();

            // Buildings (real ones)
            substractPolygons.AddRange(data.Buildings
                .SelectMany(b => TerrainPolygon.FromPolygon(b.Poly, c => new TerrainPoint((float)c.X, (float)c.Y)))
                .SelectMany(b => b.Offset(0.25f))); // Little margin

            // Roads that have an edge effect of forest
            substractPolygons.AddRange(data.Roads
                .Where(r => r.RoadType != RoadType.Trail)
                .SelectMany(r => TerrainPolygon.FromPath(r.Path.Points, RoadClearWidth(r))));

            // Priority shapes
            substractPolygons.AddRange(shapes
                .Where(s => s.Category.GroundTexturePriority < OsmShapeCategory.Forest.GroundTexturePriority
                    && !s.Category.IsBuilding
                    && s.Category != OsmShapeCategory.Building
                    && s.Category != OsmShapeCategory.Water)
                .SelectMany(g => TerrainPolygon.FromGeometry(g.Geometry, data.MapInfos.LatLngToTerrainPoint)));

            return substractPolygons;
        }

        private static List<TerrainPolygon> GetCroppedForestPolygons(MapData data, List<OsmShape> shapes)
        {
            var area = TerrainPolygon.FromRectangle(data.MapInfos.P1, data.MapInfos.P2);

            var forestShapes = shapes.Where(s => s.Category == OsmShapeCategory.Forest).ToList();

            var forestPolygonsCropped = new List<TerrainPolygon>();

            var report = new ProgressReport("ForestCrop", forestShapes.Count);
            foreach (var shape in forestShapes)
            {
                foreach (var poly in TerrainPolygon.FromGeometry(shape.Geometry, data.MapInfos.LatLngToTerrainPoint))
                {
                    foreach (var cropped in poly.ClippedBy(area))
                    {
                        forestPolygonsCropped.Add(cropped);
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            return forestPolygonsCropped;
        }

        private static float RoadClearWidth(Road r)
        {
            if (r.RoadType < RoadType.TwoLanesPrimaryRoad)
            {
                return r.Width + 8f;
            }
            if (r.RoadType < RoadType.SingleLaneDirtPath)
            {
                return r.Width + 6f;
            }
            return r.Width + 3f;
        }

        private static List<TerrainObject> wasRemoved = new List<TerrainObject>();
        private static void Remove<T>(TerrainObjectLayer objects, IEnumerable<T> toremoveList, Func<T, TerrainObject, bool> match)
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
                        wasRemoved.Add(obj);
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            Console.WriteLine($"Removed => {removed}");
        }
    }
}
