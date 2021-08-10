using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using ArmaRealMap.GroundTextureDetails;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using ArmaRealMap.TerrainData.Forests;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap.Roads
{
    internal static class RoadsBuilder
    {
        internal static void Roads(MapData data, OsmStreamSource filtered, SnapshotDb db, Config config, ObjectLibraries libs, List<OsmShape> osmShapes)
        {
            PrepareRoads(data, filtered, db);

            MergeRoads(data);
            RoadWalls(data, libs);


            var residentials = TerrainPolygon.MergeAll(osmShapes.Where(s => s.Category == OsmShapeCategory.Residential || s.Category == OsmShapeCategory.Retail || s.Category == OsmShapeCategory.Commercial).SelectMany(s => s.TerrainPolygons).ToList());
            
            var template = libs.Libraries.Where(l => l.Category == ObjectCategory.RoadSideWalk).SelectMany(l => l.Objects).FirstOrDefault();
            data.UsedObjects.Add(template.Name);
            var layer = new TerrainObjectLayer(data.MapInfos);
            var report = new ProgressReport("SideWalks", residentials.Count);

            foreach (var residential in residentials)
            {
                var areaRoads = data.Roads.Where(r => r.RoadType > RoadType.TwoLanesMotorway && r.RoadType < RoadType.SingleLaneDirtRoad && r.Path.EnveloppeIntersects(residential)).ToList();
                var areaRoadsPolys = areaRoads.SelectMany(r => r.Path.ToTerrainPolygon(r.Width + template.Depth)).ToList();
                var areaRoadsMerged = TerrainPolygon.MergeAll(areaRoadsPolys);
                var areaRoadsClipped = areaRoadsMerged.SelectMany(r => r.Intersection(residential)).ToList();

                foreach (var poly in areaRoadsClipped)
                {
                    PlaceOnPath(template, layer, poly.Shell);
                    foreach(var hole in poly.Holes)
                    {
                        PlaceOnPath(template, layer, hole);
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            
            ForestBuilder.Remove(layer, data.Roads.Where(r => r.RoadType < RoadType.SingleLaneDirtRoad), (road, tree) => tree.Poly.Centroid.Distance(road.Path.AsLineString) <= road.Width / 2);


            layer.WriteFile(data.Config.Target.GetTerrain("sidewalks.txt"));



            DebugHelper.ObjectsInPolygons(data, layer, data.Roads.SelectMany(r => r.Path.ToTerrainPolygon(r.Width)).ToList(), "sidewalks.bmp");


            SaveRoadsShp(data, config);

            if (data.Elevation != null)
            {
                AdjustElevationGrid(data);
            }

            // PreviewRoads(data);


        }

        private static void PlaceOnPath(SingleObjetInfos template, TerrainObjectLayer layer, List<TerrainPoint> path)
        {
            var points = GeometryHelper.PointsOnPathRegular(path, MathF.Floor(template.Width));
            foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
            {
                var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                var angle = (180f + (MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI)) % 360f;
                layer.Insert(new TerrainObject(template, center, angle));
            }
        }

        private static void RoadWalls(MapData data, ObjectLibraries libs)
        {
            var template = libs.Libraries.Where(l => l.Category == ObjectCategory.RoadConcreteWall).SelectMany(l => l.Objects).FirstOrDefault();
            if (template != null)
            {
                data.UsedObjects.Add(template.Name);
                var layer = new TerrainObjectLayer(data.MapInfos);

                var polys = data.Roads.Where(r => r.RoadType == RoadType.TwoLanesMotorway).SelectMany(r => r.Path.ToTerrainPolygon(r.Width)).ToList();

                var merged = TerrainPolygon.MergeAll(polys);

                foreach (var poly in merged)
                {
                    var points = GeometryHelper.PointsOnPathRegular(poly.Shell, MathF.Floor(template.Width));
                    foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                    {
                        var delta = Vector2.Normalize(segment.Second.Vector - segment.First.Vector);
                        var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                        var angle = MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI;
                        layer.Insert(new TerrainObject(template, center, angle));
                    }
                }

                layer.WriteFile(data.Config.Target.GetTerrain("roadwalls.txt"));
            }
        }

        private static void MergeRoads(MapData data)
        {
            var report = new ProgressReport("MergeRoads", data.Roads.Count);
            var todo = new HashSet<Road>(data.Roads);
            var roadsPass2 = new List<Road>();
            foreach (var road in todo.ToList())
            {
                if (todo.Contains(road))
                {
                    todo.Remove(road);
                    roadsPass2.Add(road);

                    var connectedAtLast = ConnectedAt(road, todo, road.Path.LastPoint);
                    if (connectedAtLast.Count == 1)
                    {
                        todo.Remove(connectedAtLast[0]);
                        MergeInto(road, connectedAtLast[0]);
                    }

                    var connectedAtFirst = ConnectedAt(road, todo, road.Path.FirstPoint);
                    if (connectedAtFirst.Count == 1)
                    {
                        todo.Remove(connectedAtFirst[0]);
                        MergeInto(road, connectedAtFirst[0]);
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();
            data.Roads = roadsPass2;
        }

        private static void MergeInto(Road target, Road source)
        {
            if (target.Path.LastPoint == source.Path.FirstPoint)
            {
                var a = target.Path.Points.ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.FirstPoint == source.Path.LastPoint)
            {
                var a = source.Path.Points.ToList();
                var b = target.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.LastPoint == source.Path.LastPoint)
            {
                var a = target.Path.Points.ToList();
                var b = Enumerable.Reverse(source.Path.Points).ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else if (target.Path.FirstPoint == source.Path.FirstPoint)
            {
                var a = Enumerable.Reverse(target.Path.Points).ToList();
                var b = source.Path.Points.ToList();
                target.Path = new TerrainPath(a.Concat(b.Skip(1)).ToList());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static List<Road> ConnectedAt(Road self, IEnumerable<Road> roads, TerrainPoint point)
        {
            return roads.Where(r => r != self && r.RoadType == self.RoadType && (r.Path.FirstPoint == point || r.Path.LastPoint == point)).ToList();
        }

        private static void AdjustElevationGrid(MapData data)
        {
            var cacheFile = data.Config.Target.GetCache("elevation-roads.asc");

            if (File.Exists(cacheFile))
            {
                data.Elevation.LoadFromAsc(cacheFile);
                return;
            }

            var sortedRoads = data.Roads.Where(r => r.RoadType < RoadType.SingleLaneDirtRoad)/*.OrderByDescending(r => (int)r.RoadType)*/.ToList();
            var report = new ProgressReport("ElevationRoads", sortedRoads.Count );
            var gridConstraints = new int[data.MapInfos.Size, data.MapInfos.Size];
            var rotate1 = Matrix3x2.CreateRotation(1.570796f);
            var rotate2 = Matrix3x2.CreateRotation(-1.570796f);
            var margin = new Vector2(15);

            foreach (var road in sortedRoads)
            {
                var points = GeometryHelper.PointsOnPath(road.Path.Points, 5f).ToList();

                var segments = new List<RoadSegment>(); //points.Take(points.Count - 1).Zip(points.Skip(1));

                foreach (var segment in points.Take(points.Count - 1).Zip(points.Skip(1)))
                {
                    var delta = segment.Second.Vector - segment.First.Vector;
                    var vector = Vector2.Normalize(delta);
                    var vector1 = Vector2.Transform(vector, rotate1);
                    var vector2 = Vector2.Transform(vector, rotate2);
                    var center = new TerrainPoint(Vector2.Lerp(segment.First.Vector, segment.Second.Vector, 0.5f));
                    var a = center + vector1 * road.Width;
                    var b = center + vector2 * road.Width;
                    var elevationA = data.Elevation.ElevationAt(a);
                    var elevationB = data.Elevation.ElevationAt(b);
                    segments.Add(new RoadSegment()
                    {
                        Length = delta.Length(),
                        First = segment.First,
                        Second = segment.Second,
                        Center = center,
                        A = a,
                        B = b,
                        Elevation = data.Elevation.ElevationAt(center),
                        Adjust = MathF.Abs(elevationA - elevationB) > 0.75f
                    });
                }

                if (segments.Any(s => s.Adjust))
                {
                    var medium = new float[segments.Count];
                    for(int i = 0; i < segments.Count; ++i)
                    {
                        medium[i] = segments.Skip(Math.Max(0, i - 2)).Take(5).Average(s => s.Elevation);
                    }
                    for (int i = 0; i < segments.Count; ++i)
                    {
                        segments[i].Elevation = medium[i];
                    }

                    foreach (var segment in segments.Where(s => s.Adjust))
                    {
                        var x1 = MathF.Min(segment.A.X, segment.B.X);
                        var x2 = MathF.Max(segment.A.X, segment.B.X);
                        var y1 = MathF.Min(segment.A.Y, segment.B.Y);
                        var y2 = MathF.Max(segment.A.Y, segment.B.Y);

                        var x = data.Elevation.PrepareToMutate(new TerrainPoint(x1, y1)-margin, new TerrainPoint(x2, y2)+margin, segment.Elevation - 10, segment.Elevation + 10);
                        x.Image.Mutate(p =>
                        {
                            var pixelLength = ((Vector2)(x.ToPixel(segment.First) - x.ToPixel(segment.Second))).Length();
                            p.DrawLines(x.ElevationToColor(segment.Elevation).WithAlpha(0.5f), pixelLength * 2.5f, x.ToPixel(segment.A), x.ToPixel(segment.B));
                            p.DrawLines(x.ElevationToColor(segment.Elevation), pixelLength * 1.5f, x.ToPixel(segment.A), x.ToPixel(segment.B));
                        });
                        x.Apply();
                    }
                }
                report.ReportOneDone();
            }
            
            report.TaskDone();

            data.Elevation.SaveToAsc(cacheFile);
            data.Elevation.SavePreview(data.Config.Target.GetDebug("elevation-roads.bmp"));
        }

        private static void Pass(MapData data, Image<Rgba32> img, Road road, IEnumerable<TerrainPoint> allPoints, ElevationGridArea b, float coef)
        {
            img.Mutate(p =>
            {
                var projected =
                    b.ToPixels(allPoints)
                    .Zip(allPoints.Select(p => data.Elevation.ElevationAt(p)).Select(p => b.ElevationToColor(p))).ToList();
                var previous = projected.First();
                foreach (var point in projected.Skip(1))
                {
                    var c1 = previous.Second;
                    var c2 = point.Second;
                    var brush = new LinearGradientBrush(previous.First, point.First, GradientRepetitionMode.None, new[] { new ColorStop(0, c1), new ColorStop(1, c2) });
                    p.DrawLines(brush, road.Width * coef / data.MapInfos.CellSize, previous.First, point.First);
                    previous = point;
                }
            });
        }

        private static void SaveRoadsShp(MapData data, Config config)
        {
            var features = new List<Feature>();
            foreach (var road in data.Roads)
            {
                var attributesTable = new AttributesTable();
                attributesTable.Add("ID", (int)road.RoadType);
                // Why x+200000 ? nobody really knows...
                features.Add(new Feature(road.Path.ToLineString(p => new Coordinate(p.X + 200000, p.Y)), attributesTable));
            }
            var header = ShapefileDataWriter.GetHeader(features.First(), features.Count);
            if (config.Target?.Roads != null && !Directory.Exists(config.Target?.Roads))
            {
                Directory.CreateDirectory(config.Target?.Roads);
            }
            var shapeWriter = new ShapefileDataWriter(Path.Combine(config.Target?.Roads ?? string.Empty, "roads.shp"), new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);
        }

        private static void PreviewRoads(MapData data, string file = "roads.bmp")
        {
            var report = new ProgressReport("PreviewRoads", data.Roads.Count);
            using (var img = new Image<Rgb24>(data.MapInfos.ImageryWidth, data.MapInfos.ImageryHeight, TerrainMaterial.GrassShort.GetColor(data.Config.Terrain)))
            {
                img.Mutate(d =>
                {
                    var brush = new SolidBrush(OsmShapeCategory.Road.TerrainMaterial.GetColor(data.Config.Terrain));
                    foreach (var road in data.Roads)
                    {
                        DrawHelper.DrawPath(d, road.Path, road.Width, brush, data.MapInfos);
                        report.ReportOneDone();
                    }
                });
                report.TaskDone();
                Console.WriteLine("SaveBMP");
                img.Save(file);
            }
        }

        private static void PrepareRoads(MapData data, OsmStreamSource filtered, SnapshotDb db)
        {
            var interpret = new DefaultFeatureInterpreter2();
            var osmRoads = filtered.Where(o => o.Type == OsmGeoType.Way && o.Tags != null && o.Tags.ContainsKey("highway")).ToList();
            var area = TerrainPolygon.FromRectangle(data.MapInfos.P1, data.MapInfos.P2);
            data.Roads = new List<Road>();
            var report = new ProgressReport("PrepareRoads", osmRoads.Count);
            foreach (var road in osmRoads)
            {
                var kind = OsmCategorizer.ToRoadType(road.Tags);
                if (kind != null)
                {
                    var complete = road.CreateComplete(db);
                    var count = 0;
                    foreach (var feature in interpret.Interpret(complete))
                    {
                        foreach (var path in TerrainPath.FromGeometry(feature.Geometry, data.MapInfos.LatLngToTerrainPoint))
                        {
                            if (path.Length >= 3)
                            {
                                foreach (var pathSegment in path.ClippedBy(area))
                                {
                                    data.Roads.Add(new Road()
                                    {
                                        Path = pathSegment,
                                        RoadType = kind.Value
                                    });
                                }
                            }
                        }
                        count++;
                    }
                    if (count == 0)
                    {
                        Trace.TraceWarning($"NO GEOMETRY FOR {road.Tags}");
                    }

                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        private class RoadSegment
        {
            internal bool Adjust;

            public float Length { get; set; }
            public TerrainPoint First { get; set; }
            public TerrainPoint Second { get; set; }
            public TerrainPoint Center { get; set; }
            public TerrainPoint A { get; set; }
            public TerrainPoint B { get; set; }
            public float Elevation { get; set; }
        }
    }
}
