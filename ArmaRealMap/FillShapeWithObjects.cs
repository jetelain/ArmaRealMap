using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;
using ArmaRealMap.Osm;
using NetTopologySuite.Geometries;
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

        public static void MakeForest(MapInfos area, List<OsmShape> shapes, ObjectLibrary library)
        {
            Trace.TraceInformation("MakeForest !");
            Trace.Flush();

            var objects = new SimpleSpacialIndex<TerrainObject>(
                new Vector2((float)area.StartPointUTM.Easting, (float)area.StartPointUTM.Northing),
                new Vector2(area.Width, area.Height));

            var forest = GeometryHelper.EnsureValidPolygons(
                    shapes
                    .Where(f => f.Category == OsmShapeCategory.Forest)
                    .SelectMany(f => GeometryHelper.LatLngToTerrainPolygon(area, f.Geometry))
                ).ToList();

            var density = 0.0100; // trees per m²

            RemoveOverlaps(forest);

            var areas = forest.SelectMany(f => GetFillAreas(area, f, density)).ToList();
            var generatedItems = 0;
            var report = new ProgressReport("GenerateTrees", areas.Sum(a => a.ItemsToAdd));
            foreach (var fillarea in areas)
            {
                var clusters = GenerateClusters(library, fillarea);

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
                            var obj = GetObjectToInsert(library, fillarea, clusters, point);
                            var candidate = new TerrainObject(obj, point, 0.0f);
                            var potentialConflits = objects.Search(candidate.StartPoint, candidate.EndPoint);
                            if (HasRoom(candidate, potentialConflits))
                            {
                                var toinsert = new TerrainObject(obj, point, (float)(fillarea.Random.NextDouble() * 360));
                                objects.Insert(toinsert.StartPoint, toinsert.EndPoint, toinsert);
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
            Trace.Flush();
            report.TaskDone();

            report = new ProgressReport("WriteForestTxt", objects.Count);
            using (var writer = new StreamWriter(new FileStream("forest.txt", FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in objects.Values)
                {
                    writer.WriteLine(obj.ToString());
                    report.ReportOneDone();
                }
            }
            report.TaskDone();


            using (var img = new Image<Rgb24>(area.Width, area.Height, Color.Black))
            {
                report = new ProgressReport("DrawShapes", areas.Count);
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
                img.Save("forest-places.png");
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

        private static SingleObjetInfos GetObjectToInsert(ObjectLibrary library, FillArea fillarea, SimpleSpacialIndex<SingleObjetInfos> clusters, TerrainPoint point)
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

        private static SimpleSpacialIndex<SingleObjetInfos> GenerateClusters(ObjectLibrary library, FillArea fillarea)
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
