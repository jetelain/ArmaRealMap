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
using CoordinateSharp;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Db.Impl;
using OsmSharp.Geo;
using OsmSharp.Streams;
using OsmSharp.Tags;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    class Program
    {
        private static readonly EagerLoad eagerUTM = new EagerLoad(false) { UTM_MGRS = true };

        static void Main(string[] args)
        {
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("config.json"));

            Trace.Listeners.Clear();

            Trace.Listeners.Add(new TextWriterTraceListener(@"arm.log"));

            Trace.WriteLine("----------------------------------------------------------------------------------------------------");

            var olibs = new ObjectLibraries();
            olibs.Load(config);
            File.WriteAllText(Path.Combine(config.Target?.Terrain ?? string.Empty, "library.sqf"), olibs.TerrainBuilder.GetAllSqf());

            GDTConfigBuilder.PrepareGDT(config);

            var area = MapInfos.Create(config);

            //SatelliteRawImage(config, area);

            //ElevationGridBuilder.LoadOrGenerateElevationGrid(config, area);

            BuildLand(config, area, olibs);

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


        private static void BuildLand(Config config,MapInfos area, ObjectLibraries olibs)
        {
            var startPointUTM = area.StartPointUTM;

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

                var toRender = OsmCategorizer.GetShapes(db, filtered);

                //ExportForestAsShapeFile(area, toRender);

                //PlaceIsolatedTrees(area, olibs, usedObjects, filtered);

                //PlaceBuildings(area, olibs, usedObjects, toRender);

                DrawShapes(area, startPointUTM, toRender);



            }


            var libs = olibs.TerrainBuilder.Libraries.Where(l => usedObjects.Any(o => l.Template.Any(t => t.Name==o))).Distinct().ToList();
            File.WriteAllLines("required_tml.txt", libs.Select(t => t.Name));
        }

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
                    var utmbPos = new CoordinateSharp.Coordinate(place.Latitude.Value, place.Longitude.Value, eagerUTM).UTM;
                    var pos = new PointF((float)utmbPos.Easting, (float)utmbPos.Northing);

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

        private static void ExportForestAsShapeFile(MapInfos area, List<Area> toRender)
        {
            var forest = toRender.Where(f => f.Category == AreaCategory.Forest).ToList();
            var attributesTable = new AttributesTable();
            var features = forest.SelectMany(f => ToPolygon(area, f.Geometry)).Select(f => new Feature(f, attributesTable)).ToList();
            var header = ShapefileDataWriter.GetHeader(features.First(), features.Count);
            var shapeWriter = new ShapefileDataWriter("forest.shp", new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);
        }

        private static IEnumerable<Polygon> ToPolygon(MapInfos area, Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                return ((MultiPolygon)geometry).Geometries.SelectMany(p => ToPolygon(area, p));
            }
            if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (Polygon)geometry;
                return new[] 
                { 
                    new Polygon(
                        ToTerrainBuilderRing(area, poly.ExteriorRing),
                        poly.InteriorRings.Select(h => ToTerrainBuilderRing(area, h)).ToArray()) 
                };
            }
            if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (LineString)geometry;
                if (line.IsClosed && line.Coordinates.Length > 4)
                {
                    return new[] 
                    { 
                        new Polygon(
                            new LinearRing(
                                ToTerrainBuilderPoints(area.StartPointUTM, ((LineString)geometry).Coordinates)
                                .Select(p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y))
                                .ToArray())) 
                    };
                }
            }
            return new Polygon[0];
        }

        private static LinearRing ToTerrainBuilderRing(MapInfos area, LineString line)
        {
            return new LinearRing(ToTerrainBuilderPoints(area.StartPointUTM, line.Coordinates).Select(p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)).ToArray());
        }

        private static IEnumerable<LineString> ToLineString(MapInfos area, Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var points = ToTerrainBuilderPoints(area.StartPointUTM, ((LineString)geometry).Coordinates).Where(p => area.IsInside(p)).ToList();
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
                        attributesTable.Add("ID", (int)kind); // ref
                        //attributesTable.Add("N", Get(road.Tags, "ref") ?? string.Empty);
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
            foreach (var tree in trees)
            {
                var pos = ToTerrainBuilderPoint(area.StartPointUTM, (Node)tree);  
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

        private static void PlaceBuildings(MapInfos area, ObjectLibraries olibs, HashSet<string> usedObjects, List<Area> toRender)
        {
            var result = new StringBuilder();
            var buildings = toRender.Count(b => b.Category.IsBuilding);
            var metas = toRender.Where(b => AreaCategory.BuildingCategorizers.Contains(b.Category)).ToList();

            var report = new ProgressReport("PlaceBuildings", buildings);

            foreach (var building in toRender.Where(b => b.Category.IsBuilding))
            {
                if (building.BuildingCategory == null)
                {
                    building.BuildingCategory = metas.Where(m => m.Geometry.Contains(building.Geometry)).FirstOrDefault()?.BuildingCategory ?? ObjectCategory.Residential;
                }

                var points = ToTerrainBuilderPoints(area.StartPointUTM, building.Geometry.Coordinates).ToArray();

                if (points.Any(p => !area.IsInside(p)))
                {
                    report.ReportOneDone();
                    continue;
                }

                //var box = BoundingBox.Compute(ToPixelsPoints(startPointUTM, area.Height, building.Geometry.Coordinates).ToArray());
                var box = BoundingBox.Compute(points);

                var candidates = olibs.Libraries
                    .Where(l => l.Category == building.BuildingCategory)
                    .SelectMany(l => l.Objects.Where(o => o.Fits(box, 0.75f, 1.15f)))
                    .ToList()
                    .OrderByDescending(c => c.Surface)
                    .Take(5)
                    .ToList();

                if (candidates.Count > 0)
                {
                    var random = new Random((int)Math.Truncate(box.Center.X + box.Center.Y));
                    var obj = candidates[random.Next(0, candidates.Count)];

                    var delta = obj.RotateToFit(box, 0.75f, 1.15f);
                    if (delta == 0.0f)
                    {
                        result.AppendFormat(CultureInfo.InvariantCulture, @"""{0}"";{1:0.000};{2:0.000};{3:0.000};0.0;0.0;1;0.0;",
                            obj.Name,
                            box.Center.X + obj.CX,
                            box.Center.Y + obj.CY,
                            -box.Angle + delta
                            );
                        result.AppendLine();
                        usedObjects.Add(obj.Name);
                    }
                }
                else
                {
                    Trace.WriteLine($"Nothing fits {building.BuildingCategory} {box.Width} x {box.Height}");
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            File.WriteAllText("buildings2.txt", result.ToString());
        }


        private static void DrawShapes(MapInfos area, UniversalTransverseMercator startPointUTM, List<Area> toRender)
        {
            var shapes = toRender.Count;
            var report = new ProgressReport("DrawShapes", shapes);
            using (var img = new Image<Rgb24>(area.Size * area.CellSize, area.Size * area.CellSize, TerrainMaterial.GrassShort.Color))
            {
                foreach (var item in toRender.OrderByDescending(e => e.Category.GroundTexturePriority))
                {
                    DrawGeometry(startPointUTM, img, new SolidBrush(item.Category.GroundTextureColorCode), item.Geometry);
                    report.ReportOneDone();
                }
                report.TaskDone();
                Console.WriteLine("SavePNG");
                img.Save("osm3.png");
            }
        }


        private static void DrawGeometry(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, IBrush solidBrush, Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                foreach (var geo in ((GeometryCollection)geometry).Geometries)
                {
                    DrawGeometry(startPointUTM, img, solidBrush, geo);
                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                var poly = (Polygon)geometry;
                var points = ToPixelsPoints(startPointUTM, img, poly.Shell.Coordinates).ToArray();
                try
                {
                    if (poly.Holes.Length > 0 )
                    {
                        var clip = new Rectangle(
                            (int)points.Select(p => p.X).Min() - 1,
                            (int)points.Select(p => p.Y).Min() - 1,
                            (int)(points.Select(p => p.X).Max() - points.Select(p => p.X).Min()) + 2,
                            (int)(points.Select(p => p.Y).Max() - points.Select(p => p.Y).Min()) + 2);

                        using (var dimg = new Image<Rgba32>(clip.Width, clip.Height, Color.Transparent))
                        {
                            var holes = poly.Holes.Select(h => 
                            ToPixelsPoints(startPointUTM, img, h.Coordinates)
                            .Select(p => new PointF(p.X - clip.X, p.Y -clip.Y)).ToArray()).ToList();

                            dimg.Mutate(p =>
                            {
                                p.Clear(Color.Transparent);
                                p.FillPolygon(solidBrush, points.Select(p => new PointF(p.X - clip.X, p.Y - clip.Y)).ToArray());
                                foreach (var hpoints in holes)
                                {
                                    p.FillPolygon(new ShapeGraphicsOptions() { GraphicsOptions = new GraphicsOptions() { AlphaCompositionMode = PixelAlphaCompositionMode.Xor } }, solidBrush, hpoints);
                                }
                            });
                            img.Mutate(p => p.DrawImage(dimg, new SixLabors.ImageSharp.Point(clip.X, clip.Y), 1));
                        }

                    }
                    else
                    {
                        img.Mutate(p => p.FillPolygon(solidBrush, points));
                    }
                }
                catch
                {

                }
            }
            else if (geometry.OgcGeometryType == OgcGeometryType.LineString)
            {
                var line = (LineString)geometry;
                var points = ToPixelsPoints(startPointUTM, img, line.Coordinates).ToArray();
                try
                {
                    if (line.IsClosed)
                    {
                        img.Mutate(p => p.FillPolygon(solidBrush, points));
                    }
                    else
                    {
                        img.Mutate(p => p.DrawLines(solidBrush, 6.0f, points));
                    }
                }
                catch
                {

                }
            }
            else
            {
                Console.WriteLine(geometry.OgcGeometryType);
            }
        }




        private static IEnumerable<PointF> ToPixelsPoints(UniversalTransverseMercator startPointUTM, Image<Rgb24> img, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return ToPixelsPoints(startPointUTM, img.Height, nodes);
        }

        private static IEnumerable<PointF> ToPixelsPoints(UniversalTransverseMercator startPointUTM, float height, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes
                .Select(n => new CoordinateSharp.Coordinate(n.Y, n.X, eagerUTM).UTM)
                .Select(u => new PointF(
                    (float)(u.Easting - startPointUTM.Easting),
                    (float)height - (float)(u.Northing - startPointUTM.Northing)
                ));
        }


        private static IEnumerable<PointF> ToTerrainBuilderPoints(UniversalTransverseMercator startPointUTM, IEnumerable<NetTopologySuite.Geometries.Coordinate> nodes)
        {
            return nodes
                .Select(n => new CoordinateSharp.Coordinate(n.Y, n.X, eagerUTM).UTM)
                .Select(u => new PointF(
                    (float)u.Easting,
                    (float)u.Northing
                ));
        }
        private static PointF ToTerrainBuilderPoint(UniversalTransverseMercator startPointUTM, Node node)
        {
            var coord = new CoordinateSharp.Coordinate(node.Latitude.Value, node.Longitude.Value, eagerUTM).UTM;

            return new PointF(
                    (float)coord.Easting,
                    (float)coord.Northing
                );
        }

    }
}
