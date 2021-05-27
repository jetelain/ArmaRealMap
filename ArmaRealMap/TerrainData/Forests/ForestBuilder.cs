using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            DebugHelper.Polygons(data, forestPolygonsCropped, "forest-pass1.bmp");

            RemoveOverlaps(forestPolygonsCropped);

            DebugHelper.Polygons(data, forestPolygonsCropped, "forest-pass2.bmp");

            var substractPolygons = GetPriorityPolygons(data, shapes);

            var forestPolygonsCleaned = GetForestPolygons(forestPolygonsCropped, substractPolygons);

            DebugHelper.Polygons(data, forestPolygonsCleaned, "forest-pass3.bmp");

            var objects = new FillShapeWithObjects(data, olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.ForestTree))
                    .Fill(forestPolygonsCleaned, 0.0100f, "forest-pass4.txt");

            // Remove trees at edge that may be a problem

            // Too close of building
            Remove(objects, data.Buildings, (building, tree) => tree.Poly.Distance(building.Poly) < 0.25f);

            // Too close roads
            Remove(objects, data.Roads.Where(r => r.RoadType < RoadType.SingleLaneDirtPath), (road, tree) => tree.Poly.Distance(road.Path.AsLineString) < road.Width / 2);

            // Trunc at middle of path
            Remove(objects, data.Roads.Where(r => r.RoadType >= RoadType.SingleLaneDirtPath), (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) < road.Width / 2);

            objects.WriteFile(data.Config.Target.GetTerrain("forest.txt"));

            DebugHelper.ObjectsInPolygons(data, objects, forestPolygonsCleaned, "forest-pass5.bmp");

            data.Forests = forestPolygonsCleaned;
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
            if (r.RoadType < RoadType.TwoLanesSecondaryRoad)
            {
                return r.Width * 1.5f;
            }
            return r.Width;
        }

        private static void Remove<T>(TerrainObjectLayer objects, IEnumerable<T> toremoveList, Func<T, TerrainObject, bool> match)
            where T : ITerrainGeometry
        {
            var list = toremoveList.ToList();

            var report = new ProgressReport("Remove", list.Count);

            foreach (var toremove in list)
            {
                foreach (var obj in objects.Search(toremove.MinPoint, toremove.MaxPoint))
                {
                    if (match(toremove, obj))
                    {
                        objects.Remove(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }
    }
}
