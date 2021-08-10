using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.Buildings
{
    class BuildingsBuilder
    {
        public static void PlaceBuildings(MapData data, ObjectLibraries olibs, List<OsmShape> toRender)
        {
            var pass4File = data.Config.Target.GetCache("buildings-pass4.json");

            if (File.Exists(pass4File))
            {
                data.WantedBuildings = JsonSerializer.Deserialize<IEnumerable<BuildingJson>>(File.ReadAllText(pass4File)).Select(b => new Building(b)).ToList();
            }
            else
            {
                var removed = new List<OsmShape>();
                var pass1 = BuildingPass1(data.MapInfos, toRender.Where(b => b.Category.IsBuilding).ToList(), removed);
                Preview(data, removed, pass1, "buildings-pass1.bmp");

                var pass2 = BuldingPass2(pass1, removed);
                Preview(data, removed, pass2, "buildings-pass2.bmp");

                var pass3 = BuildingPass3(removed, pass2);
                Preview(data, removed, pass3, "buildings-pass3.bmp");

                var pass4 = BuildingPass4(data.MapInfos, toRender, pass3);
                Preview(data, removed, pass4, "buildings-pass4.bmp");

                File.WriteAllText(pass4File, JsonSerializer.Serialize(pass4.Select(o => o.ToJson())));

                data.WantedBuildings = pass4;
            }

            data.Buildings = new TerrainObjectLayer(data.MapInfos);

            var report = new ProgressReport("PlaceBuildings", data.WantedBuildings.Count);
            var ok = 0;
            foreach (var building in data.WantedBuildings)
            {
                var candidates = olibs.Libraries
                    .Where(l => l.Category == building.Category)
                    .SelectMany(l => l.Objects.Where(o => o.Fits(building.Box, 0.75f, 1.15f)))
                    .ToList()
                    .OrderByDescending(c => c.Surface)
                    .Take(5)
                    .ToList();

                if (candidates.Count > 0)
                {
                    var random = new Random((int)Math.Truncate(building.Box.Center.X + building.Box.Center.Y));
                    var obj = candidates[random.Next(0, candidates.Count)];

                    var delta = obj.RotateToFit(building.Box, 0.75f, 1.15f);
                    TerrainObject terrainObj;
                    if (delta != 0.0f)
                    {
                        terrainObj = new TerrainObject(obj, building.Box.RotateM90());
                    }
                    else
                    {
                        terrainObj = new TerrainObject(obj, building.Box);
                    }
                    data.Buildings.Insert(terrainObj);
                    ok++;
                }
                else
                {
                    Trace.WriteLine($"Nothing fits {building.Category} {building.Box.Width} x {building.Box.Height}");
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            data.Buildings.WriteFile(data.Config.Target.GetTerrain("buildings.txt"));

            Console.WriteLine("{0:0.0} % buildings placed", ok * 100.0 / data.WantedBuildings.Count);
        }

        private static List<Building> BuildingPass4(MapInfos area, List<OsmShape> toRender, List<Building> pass3)
        {
            var pass4 = pass3;
            var metas = toRender
                .Where(b => OsmShapeCategory.BuildingCategorizers.Contains(b.Category) && b.Category.BuildingType != ObjectCategory.Residential)
                .Select(b => new
                {
                    BuildingType = b.Category.BuildingType,
                    Poly = GeometryHelper.LatLngToTerrainPolygon(area, b.Geometry)
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

        private static List<Building> BuildingPass1(MapInfos area, List<OsmShape> buildings, List<OsmShape> removed)
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

        private static List<Building> BuldingPass2(List<Building> pass1Builidings, List<OsmShape> removed)
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

        private static List<Building> BuildingPass3(List<OsmShape> removed, List<Building> pass2)
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

        private static ProgressReport Preview(MapData data, List<OsmShape> removed, List<Building> remainBuildings, string image)
        {
            ProgressReport report;
            using (var img = new Image<Rgb24>(data.MapInfos.ImageryWidth, data.MapInfos.ImageryHeight, TerrainMaterial.GrassShort.GetColor(data.Config.Terrain)))
            {
                var kept = remainBuildings.SelectMany(b => b.Shapes).ToList();

                report = new ProgressReport("DrawShapes", removed.Count + kept.Count);
                foreach (var item in removed)
                {
                    OsmDrawHelper.Draw(data.MapInfos, img, new SolidBrush(Color.LightGray), item);
                    report.ReportOneDone();
                }
                foreach (var item in kept)
                {
                    OsmDrawHelper.Draw(data.MapInfos, img, new SolidBrush(Color.DarkGray), item);
                    report.ReportOneDone();
                }
                report.TaskDone();

                report = new ProgressReport("DrawRect", remainBuildings.Count);
                foreach (var item in remainBuildings)
                {
                    var color = Color.White;
                    if (item.Category != null)
                    {
                        switch (item.Category)
                        {
                            case ObjectCategory.Church: color = Color.Blue; break;
                            case ObjectCategory.HistoricalFort: color = Color.Maroon; break;
                            case ObjectCategory.Industrial: color = Color.Black; break;
                            case ObjectCategory.Military: color = Color.Red; break;
                            case ObjectCategory.Residential: color = Color.Green; break;
                            case ObjectCategory.Retail: color = Color.Orange; break;
                        }

                    }

                    img.Mutate(x => x.DrawPolygon(color, 1, data.MapInfos.TerrainToPixelsPoints(item.Box.Points).ToArray()));
                    report.ReportOneDone();
                }
                report.TaskDone();

                Console.WriteLine("Save");
                img.Save(data.Config.Target.GetDebug(image));
            }

            return report;
        }
    }
}
