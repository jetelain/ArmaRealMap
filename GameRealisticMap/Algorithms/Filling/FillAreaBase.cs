using System.Collections.Concurrent;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Algorithms.Filling
{
    public abstract class FillAreaBase<TModelInfo>
    {
        protected readonly IProgressSystem progress;
        protected readonly CancellationToken cancellationToken;

        protected FillAreaBase(IProgressSystem progress)
        {
            this.progress = progress;
            this.cancellationToken = (progress as IProgressTask)?.CancellationToken ?? CancellationToken.None;
        }

        internal abstract AreaFillingBase<TModelInfo> GenerateAreaSelectData(AreaDefinition fillarea);

        public virtual int FillPolygons(RadiusPlacedLayer<TModelInfo> objects, List<TerrainPolygon> polygons)
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
                return generatedItems;
            }
        }

        private List<AreaFillingBase<TModelInfo>> GetFillAreas(IEnumerable<TerrainPolygon> polygons)
        {
            var areas = new ConcurrentBag<AreaFillingBase<TModelInfo>>();
            Parallel.ForEach(polygons, new ParallelOptions() { MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount * 3 / 4) }, poly =>
            {
                if (poly.Area > 1)
                {
                    var definition = new AreaDefinition(poly);
                    areas.Add(GenerateAreaSelectData(definition));
                }
            });
            return areas.ToList();
        }

        private void FillAreaList(List<AreaFillingBase<TModelInfo>> areas, RadiusPlacedLayer<TModelInfo> objects, IProgressInteger report, ref int generatedItems)
        {
            while (!cancellationToken.IsCancellationRequested)
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
            while (unChangedLoops < 10 && remainItems > 0 && !cancellationToken.IsCancellationRequested)
            {
                var wasChanged = false;
                for (int i = 0; i < Math.Max(10, fillarea.ItemsToAdd) && remainItems > 0; ++i)
                {
                    var point = fillarea.Area.GetRandomPointInside();
                    var obj = fillarea.SelectObjectToInsert(point);
                    var candidate = Create(obj, point, 0, 0, GetScale(fillarea.Area.Random, obj));
                    if (WillFit(candidate, fillarea.Area))
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
                progress.WriteLine($"Warning: Unable to generate all models for polygon '{fillarea.Area.Polygon}': {remainItems} remains on {fillarea.ItemsToAdd}");
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
            return new RadiusPlacedModel<TModelInfo>(new BoundingCircle(point, obj.ExclusiveRadius * scale, angle), elevation, scale, obj.Model, obj.Radius * scale);
        }

        protected virtual bool WillFit(RadiusPlacedModel<TModelInfo> candidate, AreaDefinition fillarea)
        {
            if (candidate.ExclusiveRadius == 0)
            {
                return true;
            }
            return fillarea.Polygon.DistanceFromBoundary(candidate.Center) >= candidate.ExclusiveRadius;
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