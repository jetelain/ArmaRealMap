using System.Collections.Concurrent;
using System.Diagnostics;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Algorithms.Filling
{
    public abstract class FillAreaBase<TModelInfo>
    {
        protected readonly IProgressSystem progress;

        protected FillAreaBase(IProgressSystem progress)
        {
            this.progress = progress;
        }

        internal abstract AreaFillingBase<TModelInfo> GenerateAreaSelectData(AreaDefinition fillarea);

        public virtual void FillPolygons(RadiusPlacedLayer<TModelInfo> objects, List<TerrainPolygon> polygons)
        {
            var areas = GetFillAreas(polygons.ProgressStep(progress,"Areas"));

            using (var report = progress.CreateStep("Models", areas.Sum(a => a.ItemsToAdd)))
            {
                var generatedItems = 0;
                var toprocess = areas.OrderByDescending(r => r.ItemsToAdd).ToList();
                Parallel.For(0, Math.Max(2, Environment.ProcessorCount * 3 / 4), _ =>
                {
                    FillAreaList(toprocess, objects, report, ref generatedItems);
                });
            }
        }

        private List<AreaFillingBase<TModelInfo>> GetFillAreas(IEnumerable<TerrainPolygon> polygons)
        {
            var areas = new ConcurrentBag<AreaFillingBase<TModelInfo>>();
            Parallel.ForEach(polygons, new ParallelOptions() { MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount * 3 / 4) }, poly =>
            {
                var definition = new AreaDefinition(poly);
                areas.Add(GenerateAreaSelectData(definition));
            });
            return areas.ToList();
        }

        private void FillAreaList(List<AreaFillingBase<TModelInfo>> areas, RadiusPlacedLayer<TModelInfo> objects, IProgressInteger report, ref int generatedItems)
        {
            while (true)
            {
                IDisposable? locker = null;
                AreaFillingBase<TModelInfo>? areaToProcess = null;
                lock (areas)
                {
                    if (areas.Count == 0)
                    {
                        return;
                    }
                    areaToProcess = areas.FirstOrDefault(fillarea => objects.TryLock(fillarea.Area, out locker));
                    if (areaToProcess != null)
                    {
                        areas.Remove(areaToProcess);
                    }
                }
                if (areaToProcess != null)
                {
                    using (locker)
                    {
                        FillSingleArea(objects, report, areaToProcess, ref generatedItems);
                    }
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void FillSingleArea(RadiusPlacedLayer<TModelInfo> objects, IProgressInteger report, AreaFillingBase<TModelInfo> fillarea, ref int generatedItems)
        {
            var remainItems = fillarea.ItemsToAdd;
            var unChangedLoops = 0;
            while (unChangedLoops < 10 && remainItems > 0)
            {
                var wasChanged = false;
                for (int i = 0; i < fillarea.ItemsToAdd && remainItems > 0; ++i)
                {
                    var point = fillarea.Area.GetRandomPointInside();
                    var obj = fillarea.SelectObjectToInsert(point);
                    var candidate = Create(obj, point, 0, 0, GetScale(fillarea.Area.Random, obj));
                    //FIXME: if (WillFit(candidate, fillarea.Area))
                    {
                        var potentialConflits = objects.Search(candidate.MinPoint, candidate.MaxPoint);
                        if (HasRoom(candidate, potentialConflits))
                        {
                            var toinsert = Create(obj, point, (float)(fillarea.Area.Random.NextDouble() * 360), GetElevation(fillarea.Area.Random, obj), candidate.Scale);
                            objects.Insert(toinsert);
                            wasChanged = true;
                            remainItems--;
                            report.Report(Interlocked.Increment(ref generatedItems));
                        }
                    }
                }
                if (wasChanged)
                {
                    unChangedLoops = 0;
                }
                else
                {
                    unChangedLoops++;
                }
            }
            if (remainItems > 0)
            {
                Trace.TraceWarning("Unable to generate all models for polygon '{0}': {1} remains on {2}", fillarea.Area.Polygon, remainItems, fillarea.ItemsToAdd);
                Trace.Flush();
                report.Report(Interlocked.Add(ref generatedItems, remainItems));
            }
        }

        private float GetScale(Random random, IClusterItemDefinition<TModelInfo> obj)
        {
            if (obj.MinScale != null && obj.MaxScale != null)
            {
                return (float)(obj.MinScale + ((obj.MaxScale - obj.MinScale) * random.NextDouble()));
            }
            return 1;
        }

        private static float GetElevation(Random random, IClusterItemDefinition<TModelInfo> obj)
        {
            if (obj.MaxZ != null && obj.MinZ != null)
            {
                return (float)(obj.MinZ + ((obj.MaxZ - obj.MinZ) * random.NextDouble()));
            }
            return 0;
        }

        private static RadiusPlacedModel<TModelInfo> Create(IClusterItemDefinition<TModelInfo> obj, TerrainPoint point, float angle, float elevation = 0, float scale = 1)
        {
            return new RadiusPlacedModel<TModelInfo>(new BoundingCircle(point, obj.ExclusiveRadius * scale, angle), elevation, scale, obj.Model);
        }

        private static bool WillFit(RadiusPlacedModel<TModelInfo> candidate, AreaDefinition fillarea)
        {
            return fillarea.Polygon.AsPolygon.Contains(candidate.Polygon.AsPolygon);
        }

        private static bool HasRoom(RadiusPlacedModel<TModelInfo> candidate, IReadOnlyList<RadiusPlacedModel<TModelInfo>> potentialConflits)
        {
            if (potentialConflits.Count == 0)
            {
                return true;
            }
            return potentialConflits.All(c => (c.Center.Vector - candidate.Center.Vector).Length() > (c.Radius + candidate.Radius));
        }
    }
}