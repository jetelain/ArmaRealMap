using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using ArmaRealMap.Core.ObjectLibraries;
using GameRealisticMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.Roads;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using NetTopologySuite.Geometries;

namespace ArmaRealMap.Buildings
{
    class BuildingsBuilder
    {
        public static void PlaceBuildings(MapData data, ObjectLibraries olibs, List<OsmShape> toRender)
        {
            var pass5File = Path.Combine(data.Config.Target.InternalCache, "buildings-pass5.json");
            var wasFromCache = true;

            var removed = new List<OsmShape>();

            if (File.Exists(pass5File))
            {
                data.WantedBuildings = JsonSerializer.Deserialize<IEnumerable<BuildingJson>>(File.ReadAllText(pass5File)).Select(b => new Building(b)).ToList();
            }
            else
            {
                var pass1 = DetectBuildingsBoundingRects(data.MapInfos, toRender.Where(b => b.Category.IsBuilding).ToList(), removed);
                //Preview(data, removed, pass1, "buildings-pass1.png");

                var pass2 = MergeSmallBuildings(pass1, removed);
                //Preview(data, removed, pass2, "buildings-pass2.png");

                var pass3 = RemoveCollidingBuildings(removed, pass2);
                //Preview(data, removed, pass3, "buildings-pass3.png");

                var pass4 = RoadCrop(removed, pass3, data.Roads);
                //Preview(data, removed, pass4, "buildings-pass4.png");

                var pass5 = DetectBuildingCategory(data.MapInfos, toRender, pass4);
                //Preview(data, removed, pass5, "buildings-pass5.png");

                File.WriteAllText(pass5File, JsonSerializer.Serialize(pass5.Select(o => o.ToJson())));

                data.WantedBuildings = pass5;

                wasFromCache = false;
            }

            data.Buildings = new TerrainObjectLayer(data.MapInfos);

            var nonefits = 0;
            var report = new ProgressReport("PlaceBuildings", data.WantedBuildings.Count);
            foreach (var building in data.WantedBuildings)
            {
                if (   !TryPlaceBuilding(olibs, data, building, 0.5f, 1.25f)
                    && !TryPlaceBuildingIfNotOverlapping(olibs, data, building, 1.25f, 10f))
                {
                    Trace.WriteLine($"Nothing fits {building.Category} {building.Box.Width} x {building.Box.Height}");
                    nonefits++;
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            if (!wasFromCache)
            {
                Preview(data, removed, data.WantedBuildings, "buildings-final.png");
            }
            data.Buildings.WriteFile(Path.Combine(data.Config.Target.Objects, "buildings.rel.txt"));

            Console.WriteLine("{0:0.0} % buildings placed", (data.WantedBuildings.Count - nonefits) * 100.0 / data.WantedBuildings.Count);
        }

        private static bool TryPlaceBuildingIfNotOverlapping(ObjectLibraries olibs, MapData data, Building building, float min, float max)
        {
            var candidates = GetBuildings(olibs, building, min, max);
            if (candidates.Count > 0)
            {
                var obj = PickOne(building, candidates);
                var delta = obj.RotateToFit(building.Box, min, max);
                var realbox = RealBoxAdjustedToRoad(data, obj, delta != 0.0f ? building.Box.RotateM90() : building.Box);
                if (!data.WantedBuildings.Where(b => b != building).Any(b => b.Box.Poly.Intersects(realbox.Poly))
                    && !data.Buildings.Any(b => b.Poly.Intersects(realbox.Poly)))
                {
                    foreach(var o in obj.ToObjects(realbox))
                    {
                        data.Buildings.Insert(o);
                    }
                    return true;
                }
            }
            return false;
        }


        private static bool TryPlaceBuilding(ObjectLibraries olibs, MapData data, Building building, float min, float max)
        {
            var candidates = GetBuildings(olibs, building, min, max);
            if (candidates.Count > 0)
            {
                var obj = PickOne(building, candidates);
                var delta = obj.RotateToFit(building.Box, min, max);
                var box = delta != 0.0f ? building.Box.RotateM90() : building.Box;
                if (box.Width < obj.Width || box.Height < obj.Depth) // Object is larger than wanted box
                {
                    box = RealBoxAdjustedToRoad(data, obj, box);
                }
                foreach (var o in obj.ToObjects(box))
                {
                    data.Buildings.Insert(o);
                }
                return true;
            }
            return false;
        }

        private static ObjetInfosBase PickOne(Building building, List<ObjetInfosBase> candidates)
        {
            var random = new Random((int)Math.Truncate(building.Box.Center.X + building.Box.Center.Y));
            var obj = candidates[random.Next(0, candidates.Count)];
            return obj;
        }

        private static List<ObjetInfosBase> GetBuildings(ObjectLibraries olibs, Building building, float min, float max)
        {
            var match = olibs.GetLibraries(building.Category ?? ObjectCategory.Residential)
               .SelectMany(l => l.All.Where(o => o.Fits(building.Box, min, max)))
               .ToList()
               .OrderBy(c => Math.Abs(c.Surface - building.Box.Surface))
               .ToList();
            if (match.Count > 0)
            {
                var first = match[0];
                var tolerance = first.Surface * 0.1f; // up to 10% from best match to have a bit of random
                return match.Where(m => Math.Abs(m.Surface - first.Surface) < tolerance).ToList();
            }
            return match;
        }

        private static BoundingBox RealBoxAdjustedToRoad(MapData data, ObjetInfosBase obj, BoundingBox box)
        {
            // Check if real-box intersects road
            var realbox = new BoundingBox(box.Center, obj.Width, obj.Depth, box.Angle);
            var conflicts = data.Roads
                .Where(r => r.EnveloppeIntersects(realbox))
                .Where(r => r.Polygons.Any(p => p.AsPolygon.Intersects(realbox.Poly)))
                .ToList();
            if (conflicts.Count > 0)
            {
                var dw = Math.Max(0, obj.Width - box.Width) / 2;
                var dh = Math.Max(0, obj.Depth - box.Height) / 2;
                var rotate = Matrix3x2.CreateRotation(MathF.PI * box.Angle / 180f, Vector2.Zero);
                var b1 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(dw, dh), rotate), obj.Width, obj.Depth, box.Angle);
                var b2 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(dw, -dh), rotate), obj.Width, obj.Depth, box.Angle);
                var b3 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(-dw, -dh), rotate), obj.Width, obj.Depth, box.Angle);
                var b4 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(-dw, dh), rotate), obj.Width, obj.Depth, box.Angle);
                realbox = (new[] { b1, b2, b3, b4 })
                    .Select(b => new { Box = b, Conflits = conflicts.Sum(c => c.Polygons.Sum(p => p.AsPolygon.Intersection(b.Poly).Area)) })
                    .ToList()
                    .OrderBy(b => b.Conflits).First().Box;
            }

            return realbox;
        }

        private static List<Building> RoadCrop(List<OsmShape> removed, List<Building> pass3, List<Road> roads)
        {
            var report = new ProgressReport("RoadCrop", pass3.Count);
            var pass4 = new List<Building>();
            foreach (var building in pass3)
            {
                var conflicts = roads
                    .Where(r => r.EnveloppeIntersects(building.Box))
                    .Where(r => r.Polygons.Any(p => p.AsPolygon.Intersects(building.Box.Poly)))
                    .ToList();
                if (conflicts.Count > 0)
                {
                    var result = building.Box.Polygon.SubstractAll(conflicts.SelectMany(r => r.Polygons)).ToList();
                    if (result.Count == 1)
                    {
                        var newbox = BoundingBox.ComputeInner(result[0].Shell.Skip(1));
                        if (newbox != null)
                        {
                            if (newbox.Poly.Area < building.Box.Poly.Area / 5)
                            {

                            }

                            building.Box = newbox;
                            pass4.Add(building);
                        }
                        else
                        {
                            removed.AddRange(building.Shapes);
                        }
                    }
                    else
                    {
                        removed.AddRange(building.Shapes);
                    }
                }
                else
                {
                    pass4.Add(building);
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            return pass4;
        }

        internal static IEnumerable<Polygon> LatLngToTerrainPolygon(MapInfos map, Geometry geometry)
        {
            return GeometryHelper.ToPolygon(geometry, list => map.LatLngToTerrainPoints(list).Select(p => new Coordinate(p.X, p.Y)));
        }

        private static List<Building> DetectBuildingCategory(MapInfos area, List<OsmShape> toRender, List<Building> pass3)
        {
            var pass4 = pass3;
            var metas = toRender
                .Where(b => OsmShapeCategory.BuildingCategorizers.Contains(b.Category) && b.Category.BuildingType != ObjectCategory.Residential)
                .Select(b => new
                {
                    BuildingType = b.Category.BuildingType,
                    Poly = LatLngToTerrainPolygon(area, b.Geometry)
                })
                .ToList();

            var report4 = new ProgressReport("SetCategory", pass4.Count);
            foreach (var building in pass4)
            {
                if (building.Category == null)
                {
                    var meta = metas.Where(m => m.Poly.Any(p => p.Intersects(building.Poly))).FirstOrDefault();
                    if (meta == null)
                    {
                        building.Category = ObjectCategory.Residential;
                    }
                    else
                    {
                        building.Category = meta.BuildingType;
                    }
                }
                report4.ReportOneDone();
            }
            report4.TaskDone();
            return pass4;
        }

        private static List<Building> DetectBuildingsBoundingRects(MapInfos area, List<OsmShape> buildings, List<OsmShape> removed)
        {
            var pass1 = new List<Building>();



            var report1 = new ProgressReport("BoudingRect", buildings.Count);
            foreach (var building in buildings)
            {
                if (building.Geometry.IsValid)
                {
                    var points = area.LatLngToTerrainPoints(building.Geometry.Coordinates).ToArray();
                    if (points.Any(p => !area.IsInside(p)))
                    {
                        removed.Add(building);
                        report1.ReportOneDone();
                        continue;
                    }
                    pass1.Add(new Building(building, points));
                    report1.ReportOneDone();
                }
            }
            report1.TaskDone();
            return pass1;
        }

        private static List<Building> MergeSmallBuildings(List<Building> pass1Builidings, List<OsmShape> removed)
        {
            var pass2 = new List<Building>();

            var size = 6.5f;
            var lsize = 2f;
            var mergeLimit = 100f;

            var small = pass1Builidings.Where(b => (b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize).ToList();
            var large = pass1Builidings.Where(b => !((b.Box.Width < size && b.Box.Height < size) || b.Box.Width < lsize || b.Box.Height < lsize) && b.Box.Width < mergeLimit && b.Box.Width < mergeLimit).ToList();
            var heavy = pass1Builidings.Where(b => b.Box.Width >= mergeLimit || b.Box.Height >= mergeLimit).ToList();

            var report2 = new ProgressReport("ClearHeavy", large.Count);

            foreach (var building in heavy)
            {
                removed.AddRange(small.Concat(large).Where(s => building.Poly.Contains(s.Poly)).SelectMany(b => b.Shapes));
                small.RemoveAll(s => building.Poly.Contains(s.Poly));
                large.RemoveAll(s => building.Poly.Contains(s.Poly));
                report2.ReportOneDone();
            }
            report2.TaskDone();

            report2 = new ProgressReport("MergeSmalls", large.Count);

            foreach (var building in large)
            {
                if (building.Category == ObjectCategory.Hut) continue;
                var wasUpdated = true;
                while (wasUpdated && building.Box.Width < mergeLimit && building.Box.Width < mergeLimit)
                {
                    wasUpdated = false;
                    foreach (var other in small.Where(b => b.Poly.Intersects(building.Poly)).ToList())
                    {
                        small.Remove(other);
                        building.Add(other);
                        wasUpdated = true;
                    }
                }
                report2.ReportOneDone();

            }
            report2.TaskDone();

            pass2.AddRange(large);
            pass2.AddRange(small);
            pass2.AddRange(heavy);
            return pass2;
        }

        private static List<Building> RemoveCollidingBuildings(List<OsmShape> removed, List<Building> pass2)
        {
            var pass3 = pass2.OrderByDescending(l => l.Box.Width * l.Box.Height).ToList();

            ProgressReport report;
            var merged = 0;
            var todo = pass3.ToList();
            report = new ProgressReport("RemoveCollision", todo.Count);
            while (todo.Count > 0)
            {
                var building = todo[0];
                todo.RemoveAt(0);
                bool wasChanged;
                do
                {
                    wasChanged = false;
                    foreach (var other in todo.Where(o => o.Poly.Intersects(building.Poly)).ToList())
                    {
                        var intersection = building.Poly.Intersection(other.Poly);
                        if (intersection.Area > other.Poly.Area * 0.15)
                        {
                            todo.Remove(other);
                            pass3.Remove(other);
                            removed.AddRange(other.Shapes);
                        }
                        else
                        {
                            var mergeSimulation = building.Box.Add(other.Box);
                            if (mergeSimulation.Poly.Area <= (building.Poly.Area + other.Poly.Area - intersection.Area) * 1.05)
                            {
                                building.Add(other);
                                pass3.Remove(other);
                                todo.Remove(other);
                                merged++;
                                wasChanged = true;
                            }
                        }
                    }
                    report.ReportItemsDone(report.Total - todo.Count);
                }
                while (wasChanged);
            }
            report.TaskDone();
            return pass3;
        }

        private static void Preview(MapData data, List<OsmShape> removed, List<Building> remainBuildings, string image)
        {
            DebugImage.Image(new TerrainPoint(16000, 61000), new TerrainPoint(22000, 64000), 2, System.IO.Path.Combine(data.Config.Target.Debug, image), i =>
            {
                var kept = remainBuildings.SelectMany(b => b.Shapes).ToList();
                var report = new ProgressReport("DrawShapes", removed.Count + kept.Count);
                foreach (var item in data.Roads.SelectMany(r => r.Polygons))
                {
                    i.Fill(item, new SolidBrush(Color.GreenYellow));
                }
                foreach (var item in removed)
                {
                    i.Fill(item, new SolidBrush(Color.DarkRed));
                    report.ReportOneDone();
                }
                foreach (var item in kept)
                {
                    i.Fill(item, new SolidBrush(Color.DarkGray));
                    report.ReportOneDone();
                }
                report.TaskDone();

                report = new ProgressReport("DrawRect", remainBuildings.Count);
                foreach (var item in remainBuildings)
                {
                    i.Draw(item.Box.Polygon, new SolidBrush(GetColor(item.Category)));
                    report.ReportOneDone();
                }
                report.TaskDone();

                if (data.Buildings != null)
                {
                    foreach (var building in data.Buildings)
                    {
                        var realbox = new BoundingBox(building.Center, building.Object.Width, building.Object.Depth, building.Angle);
                        i.Fill(realbox.Polygon, new SolidBrush(Color.White.WithAlpha(0.5f)));
                    }
                }

            });
        }

        private static Color GetColor(ObjectCategory? category)
        {
            var color = Color.White;
            if (category != null)
            {
                switch (category)
                {
                    case ObjectCategory.Church: color = Color.Blue; break;
                    case ObjectCategory.HistoricalFort: color = Color.Maroon; break;
                    case ObjectCategory.Industrial: color = Color.Black; break;
                    case ObjectCategory.Military: color = Color.Red; break;
                    case ObjectCategory.Residential: color = Color.Green; break;
                    case ObjectCategory.Retail: color = Color.Orange; break;
                    case ObjectCategory.Hut: color = Color.Yellow; break;
                }
            }

            return color;
        }
    }
}
