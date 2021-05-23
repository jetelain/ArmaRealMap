using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using NetTopologySuite.Geometries;
using OsmSharp;
using OsmSharp.Complete;
using OsmSharp.Db;
using OsmSharp.Geo;
using OsmSharp.Streams;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ArmaRealMap
{
    class FillShapeWithObjects
    {
        private static readonly Vector2 searchArea = new Vector2(50, 50);

        private readonly MapInfos area;
        private readonly ObjectLibrary library;

        public FillShapeWithObjects(MapInfos area, ObjectLibrary library)
        {
            this.area = area;
            this.library = library;
        }

        private static IEnumerable<FillArea> GetFillAreas(MapInfos area, NetTopologySuite.Geometries.Polygon shape, double density)
        {
            return GeometryHelper.CropPolygonToMap(area, shape).Select(cropped
                => new FillArea()
                {
                    Shape = shape,
                    CroppedShape = cropped,
                    CroppedArea = cropped.Area,
                    X1 = (int)Math.Floor(cropped.ExteriorRing.Coordinates.Min(c => c.X)),
                    X2 = (int)Math.Ceiling(cropped.ExteriorRing.Coordinates.Max(c => c.X)),
                    Y1 = (int)Math.Floor(cropped.ExteriorRing.Coordinates.Min(c => c.Y)),
                    Y2 = (int)Math.Ceiling(cropped.ExteriorRing.Coordinates.Max(c => c.Y)),
                    ItemsToAdd = (int)Math.Ceiling(cropped.Area * density),
                    Random = new Random((int)Math.Truncate(shape.Centroid.X + shape.Centroid.Y))
                });
        }

        private int generatedItems = 0;

        public void MakeForest(List<OsmShape> shapes, OsmStreamSource filtered, SnapshotDb db)
        {
            Trace.TraceInformation("MakeForest !");
            Trace.Flush();

            var objects = new SimpleSpacialIndex<TerrainObject>(
                area.P1.Vector,
                new Vector2(area.Width, area.Height));

            var forest = GeometryHelper.EnsureValidPolygons(
                    shapes
                    .Where(f => f.Category == OsmShapeCategory.Forest)
                    .SelectMany(f => GeometryHelper.LatLngToTerrainPolygon(area, f.Geometry))
                ).ToList();

            var density = 0.0100; // trees per m²

            RemoveOverlaps(forest);

            var areas = forest.SelectMany(f => GetFillAreas(area, f, density)).ToList();

            var forestPass1File = "forest.txt";

            if (!File.Exists(forestPass1File))
            {
                GenerateObjects(objects, areas);

                WriteFile(objects, forestPass1File);
            }
            else
            {
                ReadFile(objects, forestPass1File);
            }

            Remove(objects, shapes
                    .Where(f => f.Category == OsmShapeCategory.Grass)
                    .SelectMany(f => GeometryHelper.LatLngToTerrainPolygon(area, f.Geometry))
                    .ToList(),
                    ( p, o ) => IsPointInPolygon(p, new Coordinate(o.Center.X, o.Center.Y))
                    );

            Remove(objects, shapes
                    .Where(f => f.Category.IsBuilding)
                    .SelectMany(f => GeometryHelper.LatLngToTerrainPolygon(area, f.Geometry))
                    .ToList(),
                    (p, o) => o.Poly.Intersects(p)
                    );

            WriteFile(objects, "forest-pass2.txt");


            var interpret = new DefaultFeatureInterpreter2();
            var roads = filtered.Where(o => o.Type == OsmGeoType.Way && o.Tags.ContainsKey("highway")).ToList();
            var report = new ProgressReport("Roads", roads.Count);
            var delta = new Vector2(10, 10);
            foreach (var road in roads)
            {
                var kind = OsmCategorizer.ToRoadType(road.Tags);
                if (kind != null && kind.Value <= RoadType.TwoLanesConcreteRoad)
                {
                    var complete = road.CreateComplete(db);
                    foreach (var feature in interpret.Interpret(complete))
                    {
                        foreach (var linestring in GeometryHelper.LatLngToLineString(area, feature.Geometry))
                        {
                            var min = new Vector2((float)Math.Floor(linestring.Coordinates.Min(c => c.X)),
                                (float)Math.Floor(linestring.Coordinates.Min(c => c.Y))) - delta;
                            var max = new Vector2((float)Math.Ceiling(linestring.Coordinates.Max(c => c.X)),
                                (float)Math.Ceiling(linestring.Coordinates.Max(c => c.Y))) + delta;

                            foreach (var obj in objects.Search(min, max))
                            {
                                if (obj.Poly.Distance(linestring) <= Width(kind.Value))
                                {
                                    objects.Remove(obj.MinPoint.Vector, obj.MaxPoint.Vector, obj);
                                }
                            }
                        }
                    }
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            WriteFile(objects, "forest-pass3.txt");

            DrawForDebug(objects, areas, "forest-places3.png");
        }

        private double Width(RoadType value)
        {
            switch (value)
            {
                case RoadType.TwoLanesMotorway:
                case RoadType.TwoLanesPrimaryRoad:
                    return 4;
                case RoadType.TwoLanesSecondaryRoad:
                case RoadType.TwoLanesCityRoad:
                case RoadType.TwoLanesConcreteRoad:
                    return 3;
                case RoadType.SingleLaneDirtRoad:
                    return 0.75;
                case RoadType.SingleLaneDirtPath:
                    return 0.5;
                default:
                case RoadType.Trail:
                    return 0.25;
            }
        }

        private void Remove(SimpleSpacialIndex<TerrainObject> objects, List<NetTopologySuite.Geometries.Polygon> toremoveList, Func<NetTopologySuite.Geometries.Polygon, TerrainObject, bool> match)
        {
            var report = new ProgressReport("Remove", toremoveList.Count);

            foreach (var toremove in toremoveList)
            {
                var min = new Vector2((float)Math.Floor(toremove.ExteriorRing.Coordinates.Min(c => c.X)),
                    (float)Math.Floor(toremove.ExteriorRing.Coordinates.Min(c => c.Y)));
                var max = new Vector2((float)Math.Ceiling(toremove.ExteriorRing.Coordinates.Max(c => c.X)),
                    (float)Math.Ceiling(toremove.ExteriorRing.Coordinates.Max(c => c.Y)));

                foreach(var obj in objects.Search(min,max))
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

        private void DrawForDebug(SimpleSpacialIndex<TerrainObject> objects, List<FillArea> areas, string filename)
        {
            using (var img = new Image<Rgb24>(area.Width, area.Height, Color.Black))
            {
                var report = new ProgressReport("DrawShapes", areas.Count);
                foreach (var item in areas)
                {
                    DrawHelper.FillGeometry(img, new SolidBrush(Color.LightGreen), item.CroppedShape, area.TerrainToPixelsPoints);
                    report.ReportOneDone();
                }
                report.TaskDone();

                img.Mutate(p =>
                {
                    report = new ProgressReport("DrawTrees", objects.Count);
                    foreach (var obj in objects.Values)
                    {
                        p.Fill(Color.DarkGreen, new EllipsePolygon(area.TerrainToPixelsPoint(obj.Center), obj.Object.GetPlacementRadius()));
                        report.ReportOneDone();
                    }
                    report.TaskDone();
                });
                Console.WriteLine("SavePNG");
                img.Save(filename);
            }
        }

        private void GenerateObjects(SimpleSpacialIndex<TerrainObject> objects, List<FillArea> areas)
        {
            var report = new ProgressReport("GenerateTrees", areas.Sum(a => a.ItemsToAdd));
            var toprocess = areas.ToList();
            Parallel.For(0, 6, i =>
            {
                Process(toprocess, objects, report);
            });
            Trace.Flush();
            report.TaskDone();
        }

        private void WriteFile(SimpleSpacialIndex<TerrainObject> objects, string forestPass1File)
        {
            var report2 = new ProgressReport("WriteForestTxt", objects.Count);
            using (var writer = new StreamWriter(new FileStream(forestPass1File, FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in objects.Values)
                {
                    writer.WriteLine(obj.ToString(area));
                    report2.ReportOneDone();
                }
            }
            report2.TaskDone();
        }

        private void ReadFile(SimpleSpacialIndex<TerrainObject> objects, string forestPass1File)
        {
            using (var reader = new StreamReader(new FileStream(forestPass1File, FileMode.Open, FileAccess.Read)))
            {
                var report = new ProgressReport("ReadForestTxt", (int)reader.BaseStream.Length);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var tokens = line.Split(';');
                    var name = tokens[0].Trim('"');
                    var obj = library.Objects.First(o => o.Name == name);
                    var point = new TerrainPoint(
                                (float)(double.Parse(tokens[1], CultureInfo.InvariantCulture) - area.StartPointUTM.Easting),
                                (float)(double.Parse(tokens[2], CultureInfo.InvariantCulture) - area.StartPointUTM.Northing)
                                );
                    var tobj = new TerrainObject(obj,
                            point,
                            float.Parse(tokens[3], CultureInfo.InvariantCulture));
                    objects.Insert(point.Vector, tobj);
                    report.ReportItemsDone((int)reader.BaseStream.Position);
                }
                report.TaskDone();
            }
        }

        private IEnumerable<NetTopologySuite.Geometries.Polygon> ToPolygons(Geometry geometry)
        {
            if (geometry.OgcGeometryType == OgcGeometryType.MultiPolygon)
            {
                return ((MultiPolygon)geometry).Geometries.SelectMany(p => ToPolygons(p));
            }
            if (geometry.OgcGeometryType == OgcGeometryType.Polygon)
            {
                return new[]
                {
                    (NetTopologySuite.Geometries.Polygon)geometry
                };
            }
            return new NetTopologySuite.Geometries.Polygon[0];
        }

        private void Process(List<FillArea> areas, SimpleSpacialIndex<TerrainObject> objects, ProgressReport report)
        {
            while (true)
            {
                IDisposable locker = null;
                FillArea areaToProcess = null;
                lock (areas)
                {
                    if (areas.Count == 0)
                    {
                        return;
                    }
                    areaToProcess = areas.FirstOrDefault(fillarea => objects.TryLock(new Vector2(fillarea.X1 - 10, fillarea.Y1 - 10), new Vector2(fillarea.X2 + 10, fillarea.Y2 + 10), out locker));
                    if (areaToProcess != null)
                    {
                        areas.Remove(areaToProcess);
                    }
                }
                if (areaToProcess != null)
                {
                    using(locker)
                    {
                        FillArea(objects, report, areaToProcess);
                    }
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private void FillArea(SimpleSpacialIndex<TerrainObject> objects, ProgressReport report, FillArea fillarea)
        {
            var clusters = GenerateClusters(fillarea);

            var rndX1 = fillarea.X1 * 10;
            var rndX2 = fillarea.X2 * 10;
            var rndY1 = fillarea.Y1 * 10;
            var rndY2 = fillarea.Y2 * 10;

            var remainItems = fillarea.ItemsToAdd;
            var unChangedLoops = 0;
            while (unChangedLoops < 10 && remainItems > 0)
            {
                var wasChanged = false;
                for (int i = 0; i < fillarea.ItemsToAdd && remainItems > 0; ++i)
                {
                    var x = fillarea.Random.Next(rndX1, rndX2) / 10f;
                    var y = fillarea.Random.Next(rndY1, rndY2) / 10f;
                    var c = new Coordinate(x, y);
                    if (IsPointInPolygon(fillarea.Shape, c))
                    {
                        var point = new TerrainPoint(x, y);
                        var obj = GetObjectToInsert(fillarea, clusters, point);
                        var candidate = new TerrainObject(obj, point, 0.0f);
                        var potentialConflits = objects.Search(candidate.MinPoint.Vector, candidate.MaxPoint.Vector);
                        if (HasRoom(candidate, potentialConflits))
                        {
                            var toinsert = new TerrainObject(obj, point, (float)(fillarea.Random.NextDouble() * 360));
                            objects.Insert(toinsert.MinPoint.Vector, toinsert.MaxPoint.Vector, toinsert);
                            wasChanged = true;
                            remainItems--;
                            Interlocked.Increment(ref generatedItems);
                            report.ReportItemsDone(generatedItems);
                        }
                    }
                }
                if (wasChanged)
                {
                    if (unChangedLoops != 0)
                    {
                        Trace.TraceInformation("Reset unChangedLoops that was {0}", unChangedLoops);
                        Trace.Flush();
                    }
                    unChangedLoops = 0;
                }
                else
                {
                    unChangedLoops++;
                }
            }
            if (remainItems > 0)
            {
                Trace.TraceWarning("Unable to generate all trees for shape '{0}': {1} remains on {2}", fillarea.Shape, remainItems, fillarea.ItemsToAdd);
                Trace.Flush();
                generatedItems += remainItems;
                report.ReportItemsDone(generatedItems);
            }
        }

        private static bool IsPointInPolygon(NetTopologySuite.Geometries.Polygon shape, Coordinate point)
        {
            if (PointInPolygon(shape.ExteriorRing.Coordinates, point) == 1)
            {
                return shape.InteriorRings.All(hole => PointInPolygon(hole.Coordinates, point) == 0);
            }
            return false;
        }
        private static int PointInPolygon(Coordinate[] path, Coordinate pt)
        {
            //returns 0 if false, +1 if true, -1 if pt ON polygon boundary
            //See "The Point in Polygon Problem for Arbitrary Polygons" by Hormann & Agathos
            //http://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.88.5498&rep=rep1&type=pdf
            int result = 0, cnt = path.Length;
            if (cnt < 3) return 0;
            Coordinate ip = path[0];
            for (int i = 1; i <= cnt; ++i)
            {
                Coordinate ipNext = (i == cnt ? path[0] : path[i]);
                if (ipNext.Y == pt.Y)
                {
                    if ((ipNext.X == pt.X) || (ip.Y == pt.Y && ((ipNext.X > pt.X) == (ip.X < pt.X))))
                        return -1;
                }
                if ((ip.Y < pt.Y) != (ipNext.Y < pt.Y))
                {
                    if (ip.X >= pt.X)
                    {
                        if (ipNext.X > pt.X) result = 1 - result;
                        else
                        {
                            double d = (double)(ip.X - pt.X) * (ipNext.Y - pt.Y) - (double)(ipNext.X - pt.X) * (ip.Y - pt.Y);
                            if (d == 0) return -1;
                            else if ((d > 0) == (ipNext.Y > ip.Y)) result = 1 - result;
                        }
                    }
                    else
                    {
                        if (ipNext.X > pt.X)
                        {
                            double d = (double)(ip.X - pt.X) * (ipNext.Y - pt.Y) - (double)(ipNext.X - pt.X) * (ip.Y - pt.Y);
                            if (d == 0) return -1;
                            else if ((d > 0) == (ipNext.Y > ip.Y)) result = 1 - result;
                        }
                    }
                }
                ip = ipNext;
            }
            return result;
        }

        private SingleObjetInfos GetObjectToInsert(FillArea fillarea, SimpleSpacialIndex<SingleObjetInfos> clusters, TerrainPoint point)
        {
            var potential = clusters.Search(point.Vector - searchArea, point.Vector + searchArea);
            if (potential.Count == 0)
            {
                Trace.TraceWarning("No cluster at '{0}'", point);
                return library.Objects.OrderByDescending(o => o.PlacementProbability).First();
            }
            if (potential.Count == 1)
            {
                return potential[0];
            }
            return potential[fillarea.Random.Next(0, potential.Count)];
        }

        private SimpleSpacialIndex<SingleObjetInfos> GenerateClusters(FillArea fillarea)
        {
            var clusters = new SimpleSpacialIndex<SingleObjetInfos>(
                new Vector2(fillarea.X1, fillarea.Y1),
                new Vector2(fillarea.X2 - fillarea.X1 + 1, fillarea.Y2 - fillarea.Y1 + 1));
            var clusterCount = Math.Max(fillarea.ItemsToAdd, 100);
            foreach (var obj in library.Objects)
            {
                var count = clusterCount * (obj.PlacementProbability ?? 1);
                for (int i = 0; i < count; ++i)
                {
                    var x = fillarea.Random.Next(fillarea.X1, fillarea.X2);
                    var y = fillarea.Random.Next(fillarea.Y1, fillarea.Y2);
                    clusters.Insert(new Vector2(x, y), obj);
                }
            }
            return clusters;
        }

        private static void RemoveOverlaps(List<NetTopologySuite.Geometries.Polygon> forest)
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

        private static bool HasRoom(TerrainObject candidate, IReadOnlyList<TerrainObject> potentialConflits)
        {
            if (potentialConflits.Count == 0)
            {
                return true;
            }
            return potentialConflits.All(c => c.DistanceTo(candidate) > (c.Object.PlacementRadius + candidate.Object.PlacementRadius) * 0.75);
        }
    }
}
