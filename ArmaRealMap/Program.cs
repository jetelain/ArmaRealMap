using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

            Trace.Listeners.Clear();

            Trace.Listeners.Add(new TextWriterTraceListener(@"arm.log"));

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");

            var olibs = new ObjectLibraries();
            olibs.Load(config);
            File.WriteAllText(Path.Combine(config.Target?.Terrain ?? string.Empty, "library.sqf"), olibs.TerrainBuilder.GetAllSqf());

            //GDTConfigBuilder.PrepareGDT(config);

            var data = new MapData();

            var area = MapInfos.Create(config);

            data.MapInfos = area;

            //data.Elevation = ElevationGridBuilder.LoadOrGenerateElevationGrid(config, area);

            //SatelliteRawImage(config, area);

            BuildLand(config, data, area, olibs);

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");
            Trace.Flush();
        }



        private static void SatelliteRawImage(Config config, MapInfos area)
        {
            var rawSat = Path.Combine(config.Target?.Terrain ?? string.Empty, "sat-raw.png");
            if (!File.Exists(rawSat))
            {
                SatelliteImageBuilder.BuildSatImage(area, rawSat);
            }
        }


        private static void BuildLand(Config config, MapData data, MapInfos area, ObjectLibraries olibs)
        {
            var usedObjects = new HashSet<string>();
            using (var fileStream = File.OpenRead(config.OSM))
            {
                Console.WriteLine("Load OSM data...");
                var source = new PBFOsmStreamSource(fileStream);
                var db = new SnapshotDb(new MemorySnapshotDb(source));

                Console.WriteLine("Crop OSM data...");
                OsmStreamSource filtered = CropDataToArea(area, source);

                //RenderCitiesNames(config, area, filtered);

                //Roads(area, filtered, db, config);

                var shapes = OsmCategorizer.GetShapes(db, filtered);

                //ExportForestAsShapeFile(area, toRender);

                FillShapeWithObjects.MakeForest(area, shapes, olibs.Libraries.FirstOrDefault(l => l.Category == ObjectCategory.ForestTree));

                //PlaceIsolatedTrees(area, olibs, usedObjects, filtered);

                //PlaceBuildings(area, olibs, usedObjects, shapes);

                //MakeLakeDeeper(data, shapes);

                //DrawShapes(area, shapes);



            }


            var libs = olibs.TerrainBuilder.Libraries.Where(l => usedObjects.Any(o => l.Template.Any(t => t.Name==o))).Distinct().ToList();
            File.WriteAllLines("required_tml.txt", libs.Select(t => t.Name));
        }



        //private static void MakeLakeDeeper(MapData data, List<Area> shapes)
        //{
        //    var lakes = shapes.Where(s => s.Category == AreaCategory.Water && OsmCategorizer.Get(s.OsmGeo.Tags, "natural") == "water").ToList();
        //    foreach (var lake in lakes)
        //    {

        //    }
        //}

        private static void RenderCitiesNames(Config config, MapInfos area, OsmStreamSource filtered)
        {
            var places = filtered.Where(o => o.Type == OsmGeoType.Node && o.Tags.ContainsKey("place")).ToList();

            var id = 0;
            var sb = new StringBuilder();
            foreach (OsmSharp.Node place in places)
            {
                var kind = ToArmaKind(place.Tags.GetValue("place"));
                if (kind != null)
                {
                    var name = place.Tags.GetValue("name");
                    var pos = area.LatLngToTerrainPoint(place);

                    if (area.IsInside(pos))
                    {
                        sb.AppendLine(FormattableString.Invariant($@"class Item{id}
{{
    name = ""{name}"";
	position[]={{{pos.X - area.StartPointUTM.Easting:0.00},{pos.Y - area.StartPointUTM.Northing:0.00}}};
	type=""{kind}"";
	radiusA=500;
	radiusB=500;
	angle=0;
}};"));
                        id++;
                    }
                }
            }

            File.WriteAllText(Path.Combine(config.Target?.Config ?? string.Empty, "names.hpp"), sb.ToString());
        }

        private static string ToArmaKind(string place)
        {
            switch(place)
            {
                case "city": return "NameCityCapital";
                case "town": return "NameCity";
                case "village": return "NameVillage";
                case "hamlet": return "NameLocal";
            }
            return null;
        }

        private static OsmStreamSource CropDataToArea(MapInfos area, PBFOsmStreamSource source)
        {
            var left = (float)Math.Min(area.SouthWest.Longitude.ToDouble(), area.NorthWest.Longitude.ToDouble());
            var top = (float)Math.Max(area.NorthEast.Latitude.ToDouble(), area.NorthWest.Latitude.ToDouble());
            var right = (float)Math.Max(area.SouthEast.Longitude.ToDouble(), area.NorthEast.Longitude.ToDouble());
            var bottom = (float)Math.Min(area.SouthEast.Latitude.ToDouble(), area.SouthWest.Latitude.ToDouble());
            return source.FilterBox(left, top, right, bottom, true);
        }

        private static void ExportForestAsShapeFile(MapInfos area, List<OsmShape> toRender)
        {
            var forest = toRender.Where(f => f.Category == OsmShapeCategory.Forest).ToList();
            var attributesTable = new AttributesTable();
            var features = forest.SelectMany(f => GeometryHelper.LatLngToTerrainPolygon(area, f.Geometry)).Select(f => new Feature(f, attributesTable)).ToList();
            var header = ShapefileDataWriter.GetHeader(features.First(), features.Count);
            var shapeWriter = new ShapefileDataWriter("forest.shp", new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);
        }

        private static IEnumerable<LineString> ToLineString(MapInfos area, Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var points = area.LatLngToTerrainPoints(((LineString)geometry).Coordinates).Where(p => area.IsInside(p)).ToList();
                if (points.Count > 1)
                {
                    return new[] { new LineString(points.Select(p => new NetTopologySuite.Geometries.Coordinate(p.X - area.StartPointUTM.Easting + 200000, p.Y - area.StartPointUTM.Northing)).ToArray())};
                }
                return new LineString[0];
            }
            throw new ArgumentException(geometry.OgcGeometryType.ToString());
        }


        private static void Roads(MapInfos area, OsmStreamSource filtered, SnapshotDb db, Config config)
        {
            var interpret = new DefaultFeatureInterpreter2();
            var roads = filtered.Where(o => o.Type == OsmGeoType.Way && o.Tags.ContainsKey("highway")).ToList();
            var features = new List<Feature>();
            foreach (var road in roads)
            {
                var kind = OsmCategorizer.ToRoadType(road.Tags.GetValue("highway"));
                if ( kind != null)
                {
                    var complete = road.CreateComplete(db);
                    var count = 0;
                    foreach (var feature in interpret.Interpret(complete))
                    {
                        var attributesTable = new AttributesTable();
                        attributesTable.Add("ID", (int)kind);
                        foreach (var linestring in ToLineString(area, feature.Geometry))
                        {
                            var len = linestring.Length;
                            if (len >= 3)
                            {
                                features.Add(new Feature(linestring, attributesTable));
                            }
                            else
                            {
                                Console.WriteLine(kind);
                            }
                        }
                        count++;
                    }
                    if (count == 0)
                    {
                        Trace.TraceWarning($"NO GEOMETRY FOR {road.Tags}");
                    }
                }
            }
            var header = ShapefileDataWriter.GetHeader(features.First(), features.Count);
            var shapeWriter = new ShapefileDataWriter(Path.Combine(config.Target?.Roads ?? string.Empty,"roads.shp"), new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);
        }



        private static void PlaceIsolatedTrees(MapInfos area, ObjectLibraries olibs, HashSet<string> usedObjects, OsmStreamSource filtered)
        {
            var candidates = olibs.Libraries.Where(l => l.Category == ObjectCategory.IsolatedTree).SelectMany(l => l.Objects).ToList();
            var result = new StringBuilder();

            var trees = filtered.Where(o => o.Type == OsmGeoType.Node && OsmCategorizer.Get(o.Tags, "natural") == "tree").ToList();
            foreach (Node tree in trees)
            {
                var pos = area.LatLngToTerrainPoint(tree);  
                if (area.IsInside(pos))
                {
                    var random = new Random((int)Math.Truncate(pos.X + pos.Y));
                    var obj = candidates[random.Next(0, candidates.Count)];
                    result.AppendFormat(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;0.0;",
                    obj.Name,
                    pos.X,
                    pos.Y,
                    random.NextDouble() * 360.0
                    );
                    usedObjects.Add(obj.Name);
                    result.AppendLine();
                }
            }
            File.WriteAllText("trees.txt", result.ToString());
        }

        private static void PlaceBuildings(MapInfos area, ObjectLibraries olibs, HashSet<string> usedObjects, List<OsmShape> toRender)
        {
            List<Building> buildings;

            if (File.Exists("pass4.json"))
            {
                buildings = JsonSerializer.Deserialize<IEnumerable<BuildingJson>>(File.ReadAllText("pass4.json")).Select(b => new Building(b)).ToList();
            }
            else
            {

                var removed = new List<OsmShape>();
                var pass1 = BuildingPass1(area, toRender.Where(b => b.Category.IsBuilding).ToList(), removed);
                Preview(area, removed, pass1, "pass1.png");

                var pass2 = BuldingPass2(pass1, removed);
                Preview(area, removed, pass2, "pass2.png");

                var pass3 = BuildingPass3(removed, pass2);
                Preview(area, removed, pass3, "pass3.png");

                var pass4 = BuildingPass4(area, toRender, pass3);
                Preview(area, removed, pass4, "pass4.png");

                File.WriteAllText("pass4.json", JsonSerializer.Serialize(pass4.Select(o => o.ToJson())));

                buildings = pass4;
            }

            var report = new ProgressReport("PlaceBuildings", buildings.Count);
            var result = new StringBuilder();
            var ok = 0;
            foreach (var building in buildings)
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
                    /*if (delta == 0.0f)
                    {*/
                        result.AppendFormat(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;0.0;",
                            obj.Name,
                            building.Box.Center.X + obj.CX,
                            building.Box.Center.Y + obj.CY,
                            -building.Box.Angle + delta
                            );
                        result.AppendLine();
                        usedObjects.Add(obj.Name);
                    /*}*/
                    ok++;
                }
                else
                {
                    Trace.WriteLine($"Nothing fits {building.Category} {building.Box.Width} x {building.Box.Height}");
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            File.WriteAllText("buildings5.txt", result.ToString());
            Console.WriteLine("{0:0.0} % buildings placed", ok * 100.0 / buildings.Count);
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

        private static ProgressReport Preview(MapInfos area, List<OsmShape> removed, List<Building> remainBuildings, string image)
        {
            ProgressReport report;
            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize, TerrainMaterial.GrassShort.Color))
            {
                var kept = remainBuildings.SelectMany(b => b.Shapes).ToList();

                report = new ProgressReport("DrawShapes", removed.Count + kept.Count);
                foreach (var item in removed)
                {
                    OsmDrawHelper.Draw(area, img, new SolidBrush(Color.LightGray), item);
                    report.ReportOneDone();
                }
                foreach (var item in kept)
                {
                    OsmDrawHelper.Draw(area, img, new SolidBrush(Color.DarkGray), item);
                    report.ReportOneDone();
                }
                report.TaskDone();
                
                report = new ProgressReport("DrawRect", remainBuildings.Count);
                foreach (var item in remainBuildings)
                {
                    var color = Color.White;
                    if ( item.Category != null)
                    {
                        switch (item.Category)
                        {
                            case ObjectCategory.Church:         color = Color.Blue; break;
                            case ObjectCategory.HistoricalFort: color = Color.Maroon; break;
                            case ObjectCategory.Industrial:     color = Color.Black; break;
                            case ObjectCategory.Military:       color = Color.Red; break;
                            case ObjectCategory.Residential:    color = Color.Green; break;
                            case ObjectCategory.Retail:         color = Color.Orange; break;
                        }

                    }

                    img.Mutate(x => x.DrawPolygon(color, 1, area.TerrainToPixelsPoints(item.Box.Points).ToArray()));
                    report.ReportOneDone();
                }
                report.TaskDone();

                Console.WriteLine("SavePNG");
                img.Save(image);
            }

            return report;
        }

        private static void DrawShapes(MapInfos area, List<OsmShape> toRender)
        {
            var shapes = toRender.Count;
            var report = new ProgressReport("DrawShapes", shapes);
            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize, TerrainMaterial.GrassShort.Color))
            {
                foreach (var item in toRender.OrderByDescending(e => e.Category.GroundTexturePriority))
                {
                    OsmDrawHelper.Draw(area, img, new SolidBrush(item.Category.GroundTextureColorCode), item);
                    report.ReportOneDone();
                }
                report.TaskDone();
                Console.WriteLine("SavePNG");
                img.Save("osm4.png");
            }
        }


    }
}
