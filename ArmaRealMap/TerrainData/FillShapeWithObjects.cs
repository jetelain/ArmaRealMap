using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using ArmaRealMap.Core.ObjectLibraries;
using GameRealisticMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap
{
    abstract class FillShapeWithObjects<TSelectData>
    {

        protected readonly MapInfos area;
        protected readonly MapData mapData;
        private readonly ObjectLibrary library;
        protected readonly ObjectLibraries libraries;

        public FillShapeWithObjects(MapData mapData, ObjectCategory category, ObjectLibraries libraries)
        {
            this.mapData = mapData;
            this.area = mapData.MapInfos;
            this.library = libraries.GetSingleLibrary(category);
            this.libraries = libraries;
        }

        private List<FillArea> GetFillAreas(List<TerrainPolygon> polygons)
        {
            var density = library.Density ?? 0.01d;
            var areas = new List<FillArea>();
            var itemSurface = library.Objects.Sum(o => o.PlacementProbability.Value * Math.Pow(o.PlacementRadius.Value, 2) * Math.PI);
            var maxDensity = 1 / itemSurface * 0.8d; 
            if ( density > maxDensity )
            {
                Console.WriteLine($"WARNING: Density should be changed to '{maxDensity:0.00000}', instead of '{density:0.00000}', due to available objects");
                //density = maxDensity;
            }
            foreach (var poly in polygons)
            {
                var shape = poly.AsPolygon;
                areas.Add(new FillArea()
                {
                    Polygon = poly,
                    Shape = shape,
                    Area = shape.Area,
                    X1 = (int)Math.Floor(shape.ExteriorRing.Coordinates.Min(c => c.X)),
                    X2 = (int)Math.Ceiling(shape.ExteriorRing.Coordinates.Max(c => c.X)),
                    Y1 = (int)Math.Floor(shape.ExteriorRing.Coordinates.Min(c => c.Y)),
                    Y2 = (int)Math.Ceiling(shape.ExteriorRing.Coordinates.Max(c => c.Y)),
                    ItemsToAdd = (int)Math.Ceiling(shape.Area * density),
                    Random = new Random((int)Math.Truncate(shape.Centroid.X + shape.Centroid.Y)),
                    Library = library
                });
            }
            return areas;
        }

        private int generatedItems = 0;

        public TerrainObjectLayer Fill(List<TerrainPolygon> polygons, string cacheFile)
        {
            return Fill(new TerrainObjectLayer(area), polygons, cacheFile);
        }

        public TerrainObjectLayer Fill(TerrainObjectLayer objects, List<TerrainPolygon> polygons, string cacheFile)
        {
            if (library != null && library.Objects.Count > 0)
            {
                var cacheFileFullName = Path.Combine(mapData.Config.Target.InternalCache, cacheFile);

                if (!File.Exists(cacheFileFullName))
                {
                    var areas = GetFillAreas(polygons);

                    GenerateObjects(objects, areas);

                    objects.WriteFile(cacheFileFullName);
                }
                else
                {
                    objects.ReadFile(cacheFileFullName, libraries);
                }
            }
            return objects;
        }

        private void GenerateObjects(SimpleSpacialIndex<TerrainObject> objects, List<FillArea> areas)
        {
            var report = new ProgressReport("GenerateObjects", areas.Sum(a => a.ItemsToAdd));
            var toprocess = areas.ToList();
            Parallel.For(0, Environment.ProcessorCount * 3 / 4, i =>
            {
                Process(toprocess, objects, report);
            });
            Trace.Flush();
            report.TaskDone(); 
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
            var clusters = GenerateAreaSelectData(fillarea);

            var rndX1 = fillarea.X1 * 100;
            var rndX2 = fillarea.X2 * 100;
            var rndY1 = fillarea.Y1 * 100;
            var rndY2 = fillarea.Y2 * 100;

            var remainItems = fillarea.ItemsToAdd;
            var unChangedLoops = 0;
            while (unChangedLoops < 10 && remainItems > 0)
            {
                var wasChanged = false;
                for (int i = 0; i < fillarea.ItemsToAdd && remainItems > 0; ++i)
                {
                    var x = fillarea.Random.Next(rndX1, rndX2) / 100f;
                    var y = fillarea.Random.Next(rndY1, rndY2) / 100f;
                    if (fillarea.Polygon.Contains(new TerrainPoint(x, y)))
                    {
                        var point = new TerrainPoint(x, y);
                        var obj = SelectObjectToInsert(fillarea, clusters, point);
                        var candidate = new TerrainObject(obj, point, 0.0f);
                        if (WillFit(candidate, fillarea))
                        {
                            var potentialConflits = objects.Search(candidate.MinPoint.Vector, candidate.MaxPoint.Vector);
                            if (HasRoom(candidate, potentialConflits))
                            {
                                float? elevation = null;
                                if (obj.MaxZ != null && obj.MinZ != null)
                                {
                                    elevation = (float)(obj.MinZ + ((obj.MaxZ - obj.MinZ) * fillarea.Random.NextDouble()));
                                }
                                var toinsert = new TerrainObject(obj, point, (float)(fillarea.Random.NextDouble() * 360), elevation);
                                objects.Insert(toinsert.MinPoint.Vector, toinsert.MaxPoint.Vector, toinsert);
                                wasChanged = true;
                                remainItems--;
                                Interlocked.Increment(ref generatedItems);
                                report.ReportItemsDone(generatedItems);
                            }
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

        public bool MustFullFit { get; set; }

        private bool WillFit(TerrainObject candidate, FillArea fillarea)
        {
            if (MustFullFit)
            {
                return fillarea.Polygon.AsPolygon.Contains(candidate.Poly);
            }
            return true;
        }

        protected abstract SingleObjetInfos SelectObjectToInsert(FillArea fillarea, TSelectData clusters, TerrainPoint point);

        protected abstract TSelectData GenerateAreaSelectData(FillArea fillarea);

        private static bool HasRoom(TerrainObject candidate, IReadOnlyList<TerrainObject> potentialConflits)
        {
            if (potentialConflits.Count == 0)
            {
                return true;
            }
            return potentialConflits.All(c => c.DistanceTo(candidate) > (c.Object.PlacementRadius + candidate.Object.PlacementRadius)); // Adds a very little tolerance
        }
    }
}
